using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

namespace SqlAnalyzer.Api.Controllers.QueryAnalysis;


[ApiController]
[Route("api/query-analysis")]
public class QueryAnalysisController : ControllerBase
{
    private readonly IQueryAnalysisService _service;

    public QueryAnalysisController(IQueryAnalysisService service)
    {
        _service = service;
    }

    /// <summary>
    /// Выполняет EXPLAIN и сохраняет результат
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<QueryAnalysisResultDto>> Analyze([FromBody] QueryAnalysisDto request)
    {
        try
        {
            var result = await _service.AnalyzeAsync(request);
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