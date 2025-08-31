using SqlAnalyzer.Api.Controllers.Monitoring.Dto.Response;

namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IAutovacuumAnalysisService
{
    Task<AutovacuumAnalysisResponse> AnalyzeAutovacuumLastHourAsync();
}