namespace SqlAnalyzer.Api.Dto.Common;

public class SimpleDto<T>
{
    public T Data { get; init; }

    public SimpleDto(T data)
    {
        Data = data;
    }
}