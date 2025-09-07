namespace SqlAnalyzer.Api.Dal.Base;

public class Db
{
    public const string CollationName = "ru-RU-x-icu";

    public static string ContainsPattern(string expression) => $"%{expression}%";
}