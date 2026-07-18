using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Data;
using ProgramDesigner.Api.Domain.Enums;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.Services;
using ProgramDesigner.Tests.TestData;
using Xunit;

namespace ProgramDesigner.Tests.Integration;

public class ProgramServiceTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;
    private readonly ProgramService _service;

    public ProgramServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(_options);

        _service = new ProgramService(
            _context,
            new ProgramBuilderService(),
            new ValidationService(),
            new ProgramLoaderService(_context),
            new SimulationService());
    }

    public void Dispose() => _context.Dispose();

    private ProgramService CreateFreshService()
    {
        // A new DbContext instance backed by the SAME in-memory database, to
        // simulate a genuinely separate later request rather than reusing
        // in-process state that a real second HTTP call wouldn't have.
        var freshContext = new ApplicationDbContext(_options);

        return new ProgramService(
            freshContext,
            new ProgramBuilderService(),
            new ValidationService(),
            new ProgramLoaderService(freshContext),
            new SimulationService());
    }

    [Fact]
    public async Task CreateThenGet_ReturnsTheSameStructure()
    {
        var created = await _service.CreateAsync(ComputerScienceScenario.Build());

        var fetched = await _service.GetAsync(created.Program.Id);

        Assert.NotNull(fetched);
        Assert.Equal("Computer Science", fetched.Name);
        Assert.Equal(3, fetched.RootGroup.Children.Count);
    }

    [Fact]
    public async Task CreateThenGet_PreservesSiblingOrderAfterReload()
    {
        
        var created = await _service.CreateAsync(ComputerScienceScenario.Build());

        var fetched = await CreateFreshService().GetAsync(created.Program.Id);

        var topLevelTemplateIds = fetched!.RootGroup.Children.Select(c => c.TemplateId).ToList();
        Assert.Equal(["group-foundations", "group-major", "step-final-capstone"], topLevelTemplateIds);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_ForUnknownId()
    {
        var result = await _service.GetAsync(Guid.NewGuid().ToString());
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsNull_ForUnknownId()
    {
        var result = await _service.ValidateAsync(Guid.NewGuid().ToString());
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateThenValidate_ComputerScienceScenario_IsValidWithNoWarnings()
    {
        var created = await _service.CreateAsync(ComputerScienceScenario.Build());

        var validation = await CreateFreshService().ValidateAsync(created.Program.Id);

        Assert.NotNull(validation);
        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
        Assert.Empty(validation.Warnings);
    }

    [Fact]
    public async Task CreateResponse_AlreadyReportsValidationAtCreationTime()
    {
        var request = ComputerScienceScenario.Build(finalCapstonePrerequisiteTemplateId: "step-ai-capstone");

        var created = await _service.CreateAsync(request);

        Assert.Contains(created.Validation.Warnings, w => w.Code == "UNREACHABLE_PREREQUISITE");
    }

    [Fact]
    public async Task MissingPrerequisite_SurvivesReload_AndIsReportedByASeparateValidateCall()
    {
        // The core regression test for persisting unresolved prerequisites:
        // create a program with a dangling prerequisite reference, confirm
        // it's reported at creation time (easy -- that information is still
        // in memory at that point), then confirm it's STILL reported when
        // validated through a completely separate service/context instance
        // that has to reload everything from the database from scratch.
        var step = new StepRequest
        {
            TemplateId = "s1",
            Name = "Orphan",
            PrerequisiteTemplateIds = ["does-not-exist"]
        };

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var created = await _service.CreateAsync(new CreateProgramRequest { Name = "Orphan test", RootGroup = root });

        Assert.Contains(created.Validation.Errors, e => e.Code == "MISSING_PREREQUISITE");

        var reloadedValidation = await CreateFreshService().ValidateAsync(created.Program.Id);

        Assert.NotNull(reloadedValidation);
        Assert.False(reloadedValidation.IsValid);
        Assert.Contains(reloadedValidation.Errors, e => e.Code == "MISSING_PREREQUISITE");
    }

    [Fact]
    public async Task Create_PersistsEvenWhenInvalid()
    {
        // A program is saved as submitted even if it has errors -- /validate
        // is the endpoint that reports problems, so a designer doesn't lose
        // a draft just because something's still wrong with it.
        var step = new StepRequest { TemplateId = "s", Name = "S", PrerequisiteTemplateIds = ["s"] }; // self-dependency

        var root = new GroupRequest
        {
            TemplateId = "root",
            Name = "Root",
            GroupType = GroupType.All,
            Children = [step]
        };

        var created = await _service.CreateAsync(new CreateProgramRequest { Name = "Invalid but saved", RootGroup = root });

        Assert.False(created.Validation.IsValid);

        var fetched = await CreateFreshService().GetAsync(created.Program.Id);
        Assert.NotNull(fetched);
    }
}