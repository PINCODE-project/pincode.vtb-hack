


namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IPgStatAnalyzerService
{
    Task<AnalysisReportAdvanced> AnalyzeTopAsync(Guid dbConnectionId, CancellationToken cancellationToken = default);
    Task<string> EnsurePgStatStatementsInstalledAsync(bool tryCreateExtension = false, CancellationToken cancellationToken = default);
}