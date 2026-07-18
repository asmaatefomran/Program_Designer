using ProgramDesigner.Api.Domain.Entities;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.DTOs.Responses;

namespace ProgramDesigner.Api.Services.Interfaces;

public interface ISimulationService
{
    SimulateResponse Simulate(LearningProgram program, SimulateRequest request);
}