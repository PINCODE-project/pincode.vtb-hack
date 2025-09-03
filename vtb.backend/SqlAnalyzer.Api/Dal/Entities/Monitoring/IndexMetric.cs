using SqlAnalyzer.Api.Dal.Entities.Base;

public class IndexMetric : EntityBase, IEntityCreatedAt
{
    public string SchemaName { get; set; }
    
    public string TableName { get; set; }
    
    public string IndexName { get; set; }
    
    public long IndexScans { get; set; }
    public long TuplesRead { get; set; }
    public long TuplesFetched { get; set; }
    
    public string IndexSize { get; set; }
    
    public double IndexEfficiency { get; set; }
    
    public string IndexStatus { get; set; }
    
    public double BloatFactor { get; set; }
    public long SequentialScans { get; set; }
    public double SeqScanRatio { get; set; }
    
    public long LiveTuples { get; set; }
    public long DeadTuples { get; set; }
    public double DeadTupleRatio { get; set; }
    
    public DateTime CreateAt { get; set; }
}