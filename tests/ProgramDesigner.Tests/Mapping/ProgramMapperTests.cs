using System.Text.Json;
using ProgramDesigner.Api.DTOs.Mapping;
using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Services;
using ProgramDesigner.Tests.TestData;
using Xunit;

namespace ProgramDesigner.Tests.Mapping;

public class ProgramMapperTests
{
    private readonly ProgramBuilderService _builder = new();

    [Fact]
    public void ToResponse_ProducesAShapeThatActuallySerializes()
    {
        // Regression test: the domain entities (Node/Group/Step) cannot be
        // passed to System.Text.Json directly -- Node is abstract with no
        // discriminator, and Group.Children / Node.ParentGroup / Node.Prerequisites
        // form reference cycles. This proves the mapped response shape doesn't
        // have either problem.
        var buildResult = _builder.Build(ComputerScienceScenario.Build());

        var response = ProgramMapper.ToResponse(buildResult.Program);

        var exception = Record.Exception(() => JsonSerializer.Serialize<NodeResponse>(response.RootGroup));

        Assert.Null(exception);
    }

    [Fact]
    public void ToResponse_RoundTripsThroughJsonWithTypeDiscriminatorIntact()
    {
        var buildResult = _builder.Build(ComputerScienceScenario.Build());
        var response = ProgramMapper.ToResponse(buildResult.Program);

        var json = JsonSerializer.Serialize<NodeResponse>(response.RootGroup);
        var roundTripped = JsonSerializer.Deserialize<NodeResponse>(json);

        var rootGroup = Assert.IsType<GroupResponse>(roundTripped);
        Assert.Equal("group-root", rootGroup.TemplateId);

        // Major -> should still be a GroupResponse, its children still Steps/Groups
        var major = Assert.IsType<GroupResponse>(rootGroup.Children.Single(c => c.TemplateId == "group-major"));
        Assert.IsType<GroupResponse>(major.Children.Single(c => c.TemplateId == "group-ai"));
    }

    [Fact]
    public void ToResponse_PreservesChildOrder()
    {
        var buildResult = _builder.Build(ComputerScienceScenario.Build());
        var response = ProgramMapper.ToResponse(buildResult.Program);

        var topLevelTemplateIds = response.RootGroup.Children.Select(c => c.TemplateId).ToList();

        Assert.Equal(["group-foundations", "group-major", "step-final-capstone"], topLevelTemplateIds);
    }

    [Fact]
    public void ToResponse_IncludesPrerequisiteTemplateIds()
    {
        var buildResult = _builder.Build(ComputerScienceScenario.Build());
        var response = ProgramMapper.ToResponse(buildResult.Program);

        var major = (GroupResponse)response.RootGroup.Children.Single(c => c.TemplateId == "group-major");

        Assert.Equal(["group-foundations"], major.PrerequisiteTemplateIds);
    }
}