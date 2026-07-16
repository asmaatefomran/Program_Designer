using ProgramDesigner.Api.Services.Models;
using ProgramDesigner.Api.DTOs.Requests;

namespace ProgramDesigner.Api.Services.Interfaces;

public interface IProgramBuilderService
{
    BuildResult Build(CreateProgramRequest request);
}