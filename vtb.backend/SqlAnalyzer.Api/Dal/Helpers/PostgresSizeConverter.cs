namespace SqlAnalyzer.Api.Dal.Helpers;

/// <summary>
/// 
/// </summary>
public static class PostgresSizeConverter
{
    private static readonly Dictionary<string, long> SizeUnits = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
    {
        { "bytes", 1L },
        { "b", 1L },
        { "kb", 1024L },
        { "mb", 1024L * 1024 },
        { "gb", 1024L * 1024 * 1024 },
        { "tb", 1024L * 1024 * 1024 * 1024 },
        { "pb", 1024L * 1024 * 1024 * 1024 * 1024 }
    };

    /// <summary>
    /// Преобразует строковое представление размера из PostgreSQL в long (байты)
    /// </summary>
    /// <param name="postgresSize">Строка размера в формате PostgreSQL (например: "125 MB", "1.5GB")</param>
    /// <returns>Размер в байтах</returns>
    public static long ParsePostgresSizeToBytes(string postgresSize)
    {
        if (string.IsNullOrWhiteSpace(postgresSize))
            return 0L;

        // Удаляем лишние пробелы и приводим к нижнему регистру
        var sizeStr = postgresSize.Trim().ToLower();
        
        // Если строка уже содержит "bytes" или только число
        if (sizeStr.EndsWith("bytes"))
        {
            var numberPart = sizeStr.Replace("bytes", "").Trim();
            if (long.TryParse(numberPart, out var bytes))
                return bytes;
            return 0L;
        }

        // Ищем числовую часть и единицу измерения
        int unitStartIndex = -1;
        for (int i = 0; i < sizeStr.Length; i++)
        {
            if (!char.IsDigit(sizeStr[i]) && sizeStr[i] != '.' && sizeStr[i] != ',')
            {
                unitStartIndex = i;
                break;
            }
        }

        if (unitStartIndex == -1)
        {
            // Только число без единицы измерения - предполагаем байты
            if (double.TryParse(sizeStr, out var bytes))
                return (long)bytes;
            return 0L;
        }

        // Разделяем число и единицу измерения
        var numberString = sizeStr.Substring(0, unitStartIndex).Trim();
        var unitString = sizeStr.Substring(unitStartIndex).Trim();

        // Парсим число (может быть с точкой)
        if (!double.TryParse(numberString, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var number))
        {
            return 0L;
        }

        // Находим соответствующую единицу измерения
        foreach (var unit in SizeUnits)
        {
            if (unitString.StartsWith(unit.Key))
            {
                return (long)(number * unit.Value);
            }
        }

        // Если единица измерения не распознана, предполагаем байты
        return (long)number;
    }

    /// <summary>
    /// Преобразует байты обратно в строковый формат PostgreSQL
    /// </summary>
    public static string ConvertBytesToPostgresSize(long bytes)
    {
        string[] suffixes = { "bytes", "kB", "MB", "GB", "TB", "PB" };
        int counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:0.##} {suffixes[counter]}";
    }

    // Примеры использования:
    // ParsePostgresSizeToBytes("125 MB") → 131072000
    // ParsePostgresSizeToBytes("1.5 GB") → 1610612736
    // ParsePostgresSizeToBytes("1024") → 1024
    // ParsePostgresSizeToBytes("256 bytes") → 256
}