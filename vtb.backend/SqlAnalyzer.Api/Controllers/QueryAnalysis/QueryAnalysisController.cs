using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.QueryAnalysis;
using SqlAnalyzer.Api.Services.QueryAnalysis.Interfaces;

namespace SqlAnalyzer.Api.Controllers.QueryAnalysis;


[ApiController]
[Route("api/queries")]
public class QueryAnalysisController : ControllerBase
{
    private readonly IQueryService _service;

    public QueryAnalysisController(IQueryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Поиск всех сохраненных запросов
    /// </summary>
    [HttpGet("find")]
    public async Task<IReadOnlyCollection<QueryDto>> Find([FromQuery] QueriesFindDto dto)
    {
        var result = await _service.Find(dto);
        return result;
    }

    /// <summary>
    /// Только создает запись запроса и его эксплейна, выводит созданный id
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<SimpleDto<Guid>>> Create([FromBody] QueryCreateDto request)
    {
        try
        {
            var result = await _service.Create(request);
            return Ok(new SimpleDto<Guid>(result));
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
    
    /// <summary>
    /// По Id запроса выдает его содержимое и результат EXPLAIN
    /// </summary>
    [HttpGet("{queryId:guid}")]
    public async Task<ActionResult<QueryDto>> Get([FromRoute] Guid queryId)
    {
        try
        {
            var result = await _service.Get(queryId);
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
    
    /// <summary>
    /// Анализирует запрос алгоритмически + по флагу useLlm может выдать рекомендации от LLM и оптимизированный запрос
    /// </summary>
    [HttpPost("{queryId:guid}/analyze")]
    public async Task<ActionResult<QueryAnalysisResultDto>> Analyze([FromRoute] Guid queryId, [FromQuery] bool useLlm = false)
    {
        try
        {
            var result = await _service.Analyze(queryId, useLlm);
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
    
    /// <summary>
    /// Анализирует запрос алгоритмически + по флагу useLlm может выдать рекомендации от LLM и оптимизированный запрос
    /// </summary>
    [HttpPost("{queryId:guid}/analyze-custom")]
    public async Task<ActionResult<QueryAnalysisResultDto>> AnalyzeCustom([FromRoute] Guid queryId, [FromQuery] bool useLlm = false)
    {
        try
        {
            var result = await _service.Analyze(queryId, useLlm);
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