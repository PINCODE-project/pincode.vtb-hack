namespace SqlAnalyzer.Api.Services.SubstituteValuesToSql.Interfaces;

public interface ISubstituteValuesToSqlServer
{
    Task<string> SubstituteValuesToSql(SubstituteValuesToSqlRequestDto dto);
}