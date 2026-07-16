namespace ProgramDesigner.Api.Domain.Entities;

public class NodePrerequisite
{
    // Dependent physical node
    public required string NodeId { get; init; }
    public required Node Node { get; init; }

    // Physical instance chosen during build
    public required string PrerequisiteId { get; init; }
    public required Node Prerequisite { get; init; }

    // Logical requirement (shared by all clones)
    public required string PrerequisiteTemplateId { get; init; }
}