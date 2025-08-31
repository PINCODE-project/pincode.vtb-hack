using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

public class TempFilesStatsDal : EntityBase, IEntityCreatedAt
{
    public long TempFiles { get; set; }
    public long TempBytes { get; set; }
    public DateTime CreateAt { get; set; }
}