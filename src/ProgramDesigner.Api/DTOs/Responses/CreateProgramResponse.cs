using ProgramDesigner.Api.Domain.Entities;
namespace ProgramDesigner.Api.DTOs.Responses;


public class CreateProgramResponse
{
    public required ProgramResponse Program { get; init; }

    public required ValidationResponse Validation { get; init; }
}