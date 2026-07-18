namespace ProgramDesigner.Api.DTOs.Responses;

public class NodeStateResponse
{
    public required string Id { get; init; }
    public required string TemplateId { get; init; }
    public required string Name { get; init; }

    // "complete" | "unlocked" | "blocked"
    public required string Status { get; init; }

    public string? Reason { get; init; }
}