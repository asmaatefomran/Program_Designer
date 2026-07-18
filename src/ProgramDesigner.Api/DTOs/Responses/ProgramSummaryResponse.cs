namespace ProgramDesigner.Api.DTOs.Responses;


public class ProgramSummaryResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedAt { get; init; }
}