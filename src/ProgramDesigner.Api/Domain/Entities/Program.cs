namespace ProgramDesigner.Api.Domain.Entities;

public class Program
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required Group RootGroup { get; init; }
}