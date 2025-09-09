using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Monitoring.Services.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Monitoring;

[ApiController]
[Route("api/index")]
public class IndexAnalyzeController  : ControllerBase
{
    private readonly IIndexAnalysisService _analysisService;

    public IndexAnalyzeController(IIndexAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    [HttpGet("recommendations")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IndexAnalysisResult))]
    public async Task<ActionResult<IndexAnalysisResult>> GetRecommendationsAsync([FromQuery] Guid dbConnectionId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var recommendations = await _analysisService.GetFullAnalysisAsync(dbConnectionId, startDate, endDate);
        return Ok(recommendations);
    }
}