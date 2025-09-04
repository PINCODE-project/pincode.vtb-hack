using SqlAnalyzer.Api.Dal.Entities.Base;

public class IndexMetric : EntityBase, IEntityCreatedAt
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
    public string IndexName { get; set; }
    public long IndexScans { get; set; }
    public long IndexSize { get; set; }
    public long TuplesRead { get; set; }
    public long TuplesFetched { get; set; }
    public double Efficiency { get; set; }
    
    public DateTime CreateAt { get; set; }
}