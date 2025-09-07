namespace SqlAnalyzerLib.SqlStaticAnalysis.Models;

/// <summary>
/// DTO, описывающее SQL-запрос для анализа.
/// </summary>
public record SqlQuery(string Text, string? Database = null, string? Schema = null);