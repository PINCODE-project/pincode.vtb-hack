using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Services.SubstituteValuesToSql;
using SqlAnalyzer.Api.Services.SubstituteValuesToSql.Interfaces;

namespace SqlAnalyzer.Api.Controllers.SubstituteValuesToSql;

[ApiController]
[Route("api/substitute-values-to-sql")]
public class SubstituteValuesToSqlController : ControllerBase
{
    private readonly ISubstituteValuesToSqlServer _service;

    public SubstituteValuesToSqlController(ISubstituteValuesToSqlServer service)
    {
        _service = service;
    }

    [HttpPost("substitute")]
    public async Task<ActionResult<string>> Substitute([FromBody] SubstituteValuesToSqlRequestDto request)
    {
        try
        {
            var result = await _service.SubstituteValuesToSql(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}