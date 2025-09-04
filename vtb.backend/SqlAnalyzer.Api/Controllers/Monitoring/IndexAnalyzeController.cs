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
    public async Task<IActionResult> GetRecommendationsAsync()
    {
        var recommendations = await _analysisService.GetFullAnalysisAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        return Ok(recommendations);
    }
}