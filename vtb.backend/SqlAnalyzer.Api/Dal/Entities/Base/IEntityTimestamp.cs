namespace SqlAnalyzer.Api.Dal.Entities.Base;

public interface IEntityTimestamp
{
    DateTime CreateAt { get; }
    DateTime UpdateAt { get; }
}