namespace ProgramDesigner.Api.Domain.Entities;

public class Program
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; }

    public required Group RootGroup { get; init; }
}