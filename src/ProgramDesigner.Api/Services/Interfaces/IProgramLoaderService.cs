using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Services.Interfaces;

public interface IProgramLoaderService
{
    Task<LearningProgram?> LoadProgramAsync(string programId);
}