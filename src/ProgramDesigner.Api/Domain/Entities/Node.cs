namespace ProgramDesigner.Api.Domain.Entities;

public abstract class Node
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public List<string> PrerequisiteIds { get; init; } = [];
}