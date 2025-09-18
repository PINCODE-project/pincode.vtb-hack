using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.SqlAnalyzeRule;
using SqlAnalyzer.Api.Services.Algorithm.Interfaces;

namespace SqlAnalyzer.Api.Controllers.Algorithm;

public class SqlAnalyzeRuleController : ControllerBase
{
    private readonly ISqlAnalyzeRuleService _service;

    public SqlAnalyzeRuleController(ISqlAnalyzeRuleService service)
    {
        _service = service;
    }

    [HttpGet("find")]
    public async Task<IReadOnlyCollection<SqlAnalyzeRuleDto>> Find([FromQuery] int? skip, [FromQuery] int? take)
    {
        var result = await _service.Find(skip, take);
        return result;
    }
    
    [HttpPost]
    public async Task<SimpleDto<Guid>> Create([FromBody] [Required] SqlAnalyzeRuleCreateDto dto)
    {
        var id = await _service.Create(dto);
        return new SimpleDto<Guid>(id);
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