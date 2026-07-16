using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.DTOs.Responses;
using ProgramDesigner.Api.Domain.Entities;

namespace ProgramDesigner.Api.Services.Interfaces;

public interface IProgramService
{
    Task<ProgramResponse?> GetAsync(string id);

    Task<CreateProgramResponse> CreateAsync(CreateProgramRequest request);

    Task<ValidationResponse?> ValidateAsync(string id);
}