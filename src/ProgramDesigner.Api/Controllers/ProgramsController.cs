using Microsoft.AspNetCore.Mvc;
using ProgramDesigner.Api.DTOs.Requests;
using ProgramDesigner.Api.Services.Interfaces;
using ProgramDesigner.Api.DTOs.Responses;

namespace ProgramDesigner.Api.Controllers;

[ApiController]
[Route("programs")]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramsController(
        IProgramService programService)
    {
        _programService = programService;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateProgramResponse>> Create([FromBody]CreateProgramRequest request)
    {
        var result =  await _programService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new {id = result.Program.Id}, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProgramSummaryResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _programService.GetAllAsync(page, pageSize);
        return Ok(result);
    }
    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ProgramResponse>> GetById(string id)
    {
        var program = await _programService.GetAsync(id);
        return program is null? NotFound() : Ok(program);
    }
    
    
    [HttpPost("{id}/validate")]
    public async Task<ActionResult<ValidationResponse>> Validate(string id)
    {
        var validation = await  _programService.ValidateAsync(id);
        return validation is null ? NotFound() : Ok(validation);
    }

    [HttpPost("{id}/simulate")]
    public async Task<ActionResult<SimulateResponse>> Simulate(string id, [FromBody] SimulateRequest request)
    {
        var result = await _programService.SimulateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }
    
}