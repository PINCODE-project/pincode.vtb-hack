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
    /// Сохраняет подключение (без проверки).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SimpleDto<Guid>>> SaveConnection([FromBody] DbConnectionCreateDto request)
    {
        var result = await _service.SaveAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Проверяет доступность подключения.
    /// </summary>
    [HttpPost("{dbConnectionId:guid}/check")]
    public async Task<ActionResult<DbConnectionCheckDto>> CheckConnection([FromRoute] Guid dbConnectionId)
    {
        var result = await _service.CheckAsync(dbConnectionId);
        return Ok(result);
    }
}