using ProgramDesigner.Api.Domain.Entities;
namespace ProgramDesigner.Api.Domain.Entities;


public class LearningProgram
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string RootGroupId { get; init; }

    public Group RootGroup { get; set; } = null!;
    
}