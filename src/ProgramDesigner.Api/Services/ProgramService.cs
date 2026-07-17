using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Api.Data;
using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.DTOs.Mapping;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Services.Interfaces;
using ProgramDesigner.Api.Services.Models;

namespace ProgramDesigner.Api.Services;

public class ProgramService : IProgramService
{
    private readonly ApplicationDbContext _context;
    private readonly IProgramBuilderService _builder;
    private readonly IValidationService _validator;
    private readonly IProgramLoaderService _loader;

    public ProgramService(
        ApplicationDbContext context,
        IProgramBuilderService builder,
        IValidationService validator,
        IProgramLoaderService loader)
    {
        _context = context;
        _builder = builder;
        _validator = validator;
        _loader = loader;
    }

    public async Task<CreateProgramResponse> CreateAsync(CreateProgramRequest request)
    {
        var buildResult = _builder.Build(request);
        var validation = _validator.Validate(buildResult);

        _context.Programs.Add(buildResult.Program);
        
        foreach(var unresolved in buildResult.UnresolvedPrerequisites)
        {
            _context.MissingPrerequisites.Add(new MissingPrerequisiteRecord
            {
                Id = Guid.NewGuid().ToString(),
                ProgramId = buildResult.Program.Id,
                NodeId = unresolved.Node.Id,
                MissingPrerequisiteTemplateId =  unresolved.MissingPrerequisiteTemplateId
            });
        }
        await _context.SaveChangesAsync();

        return new CreateProgramResponse
        {
            Program = ProgramMapper.ToResponse(buildResult.Program),
            Validation = validation
        };
    }

    public async Task<ProgramResponse?> GetAsync(string id)
    {
        var program = await _loader.LoadProgramAsync(id);
        return program is null? null: ProgramMapper.ToResponse(program);
    }
    
    public async Task<ValidationResponse?> ValidateAsync(string id)
    {
        var program = await _loader.LoadProgramAsync(id);
        if (program is null)
            return null;
        
        var missingRecords = await  _context.MissingPrerequisites
            .Where(m => m.ProgramId == id)
            .ToListAsync();

        var nodesById = FlattenNodes(program.RootGroup).ToDictionary(n => n.Id);
        
        var unresolvedPrerequisites = missingRecords
            .Where(m => nodesById.ContainsKey(m.NodeId))
            .Select(m => new UnresolvedPrerequisite
            {
                Node = nodesById[m.NodeId],
                MissingPrerequisiteTemplateId =  m.MissingPrerequisiteTemplateId
            })
            .ToList();

        return _validator.Validate(new BuildResult
        {
            Program = program,
            UnresolvedPrerequisites = unresolvedPrerequisites
        });
    }

    public static IEnumerable<Node> FlattenNodes(Node node)
    {
        yield return node;

        if (node is Group group)
        {
            foreach (var child in  group.Children)
            {
                foreach (var descendant in FlattenNodes(child))
                {
                    yield return descendant;
                }
                
            }
          
        }
    }
}