using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

public class CacheHitStats : EntityBase, IEntityCreatedAt
{
    public long BlksHit { get; set; }
    public long BlksRead { get; set; }
    public decimal CacheHitRatio { get; set; }
    public DateTime CreateAt { get; set; }
}