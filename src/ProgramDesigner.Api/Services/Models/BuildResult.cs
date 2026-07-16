using ProgramDesigner.Api.Domain.Entities;
namespace ProgramDesigner.Api.Services.Models;

public class BuildResult
{
    public required LearningProgram Program { get; init; }
}