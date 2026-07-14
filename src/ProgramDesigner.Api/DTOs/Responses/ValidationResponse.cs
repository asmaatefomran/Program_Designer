namespace ProgramDesigner.Api.DTOs.Responses;

public class ValidationResponse
{
    public bool IsValid { get; init; }

    public List<ValidationIssueResponse> Errors { get; init; } = [];
    
    public List<ValidationIssueResponse> Warnings { get; init; } = [];
}