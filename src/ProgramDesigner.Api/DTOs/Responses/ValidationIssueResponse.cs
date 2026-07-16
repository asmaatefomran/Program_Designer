namespace ProgramDesigner.Api.DTOs.Responses;

public class ValidationIssueResponse
{
    public required string Code { get; init; }

    public required string NodeId { get; init; }

    public required string Message { get; init; }
}