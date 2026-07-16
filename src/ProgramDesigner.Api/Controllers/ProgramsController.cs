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
    
}