namespace ProgramDesigner.Api.DTOs.Requests;

public class SimulateRequest
{
    
    public Dictionary<string, List<string>> Choices { get; init; } = new();

    public List<string> CompletedStepTemplateIds { get; init; } = new();
}