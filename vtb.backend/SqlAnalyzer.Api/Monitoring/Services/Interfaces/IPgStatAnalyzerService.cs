


namespace SqlAnalyzer.Api.Monitoring.Services.Interfaces;

public interface IPgStatAnalyzerService
{
    Task<AnalysisReportAdvanced> AnalyzeTopAsync(Guid dbConnectionId, int limit = 50, bool includeExplain = false, CancellationToken cancellationToken = default);
    Task<string> EnsurePgStatStatementsInstalledAsync(bool tryCreateExtension = false, CancellationToken cancellationToken = default);
}