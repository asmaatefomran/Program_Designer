namespace ProgramDesigner.Api.DTOs.Requests;

public class CreateProgramRequest
{
    public required string Name { get; init; }

    public required GroupRequest RootGroup { get; init; }
}