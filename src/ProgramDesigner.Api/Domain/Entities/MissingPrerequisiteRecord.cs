namespace ProgramDesigner.Api.Domain.Entities;

public class MissingPrerequisiteRecord
{
    public required string Id { get; init; }
    
    public required string ProgramId { get; init; }
    
    public required string NodeId { get; init; }
    
    public required string MissingPrerequisiteTemplateId { get; init; }
}