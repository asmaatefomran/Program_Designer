namespace ProgramDesigner.Api.Domain.Entities;

public abstract class Node
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; }

    public List<Guid> PrerequisiteIds { get; init; } = [];
}