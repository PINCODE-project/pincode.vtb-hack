using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

public class TableStatictics : EntityBase, IEntityCreatedAt
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }

    public long CountSeqScan { get; set; }
    public long TuplesReadCountSeqScan { get; set; }
    public long IndexCountSeqScan { get; set; }
    public long TuplesFetchedIndexScan { get; set; }
    public decimal IndexUsageRatio { get; set; }

    public DateTime CreateAt { get; set; }
    public Guid DbConnectionId { get; set; }
}