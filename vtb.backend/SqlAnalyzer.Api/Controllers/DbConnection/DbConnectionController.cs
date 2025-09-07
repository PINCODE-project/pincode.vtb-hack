using Microsoft.AspNetCore.Mvc;
using SqlAnalyzer.Api.Dto.Common;
using SqlAnalyzer.Api.Dto.DbConnection;
using SqlAnalyzer.Api.Services.DbConnection.Interfaces;

namespace SqlAnalyzer.Api.Controllers.DbConnection;

[ApiController]
[Route("api/db-connections")]
public class DbConnectionController : ControllerBase
{
    private readonly IDbConnectionService _service;

    public DbConnectionController(IDbConnectionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Ищет все коннекшены
    /// </summary>
    [HttpGet("find")]
    public async Task<IReadOnlyCollection<DbConnectionDto>> Find([FromQuery] DbConnectionFindDto dto)
    {
        var result = await _service.Find(dto);
        return result;
    }

    /// <summary>
    /// Сохраняет подключение (без проверки).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SimpleDto<Guid>>> Save([FromBody] DbConnectionCreateDto dto)
    {
        var result = await _service.SaveAsync(dto);
        return Ok(result);
    }
    
    /// <summary>
    /// Обновляет
    /// </summary>
    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] DbConnectionUpdateDto dto)
    {
        await _service.Update(dto);
        return NoContent();
    }
    
    
    /// <summary>
    /// Проверяет доступность подключения.
    /// </summary>
    [HttpGet("{dbConnectionId:guid}/check")]
    public async Task<ActionResult<DbConnectionCheckDto>> Check([FromRoute] Guid dbConnectionId)
    {
        var result = await _service.CheckAsync(dbConnectionId);
        return Ok(result);
    }
    
    /// <summary>
    /// Удаляет
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute]Guid id)
    {
        await _service.Delete(id);
        return NoContent();
    }
}