using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.SqlAnalyzeRule;
using SqlAnalyzer.Api.Services.Algorithm.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Algorithm;

[ApiController]
[Route("api/sql-analyzer")]
public class SqlAnalyzeRuleController : ControllerBase
{
    private readonly ISqlAnalyzeRuleService _service;

    public SqlAnalyzeRuleController(ISqlAnalyzeRuleService service)
    {
        _service = service;
    }

    [HttpGet("find")]
    public async Task<IReadOnlyCollection<SqlAnalyzeRuleDto>> Find([FromQuery] int? skip, [FromQuery] int? take, [FromQuery] IReadOnlyCollection<Guid>? ids)
    {
        var result = await _service.Find(ids, skip, take);
        return result;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] [Required] SqlAnalyzeRuleCreateDto dto)
    {
        var id = await _service.Create(dto);
        return Ok(new SimpleDto<Guid>(id));
    }

    [HttpPatch]
    public async Task<ActionResult> Update([FromBody] [Required] SqlAnalyzeRuleUpdateDto dto)
    {
        await _service.Update(dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}