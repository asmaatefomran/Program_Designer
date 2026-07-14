using Microsoft.AspNetCore.Mvc;

namespace ProgramDesigner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Program Designer API is running.");
    }
}