namespace ProgramDesigner.Api.DTOs.Responses;

public class SimulateResponse
{
    public required List<NodeStateResponse> Complete { get; init; }
    public required List<NodeStateResponse> Unlocked { get; init; }
    public required List<NodeStateResponse> Blocked { get; init; }
}