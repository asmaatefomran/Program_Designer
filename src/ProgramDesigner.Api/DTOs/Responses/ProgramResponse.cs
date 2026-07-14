using ProgramDesigner.Api.DTOs.Requests;
namespace ProgramDesigner.Api.DTOs.Responses;

public class ProgramResponse
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required GroupRequest RootGroup { get; init; }
}