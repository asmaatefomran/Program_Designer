using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Services.Models;

namespace ProgramDesigner.Api.Services.Interfaces;

public interface IValidationService
{
    ValidationResponse Validate(BuildResult buildResult);
}