using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

/// <summary>
/// Статистика автовакуума 
/// </summary>
public class AutovacuumStat : EntityBase, IEntityCreatedAt
{
    [Required]
    public DateTime CreateAt { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string SchemaName { get; set; } = string.Empty;
        
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;
        
    public long LiveTuples { get; set; }
    public long DeadTuples { get; set; }
        
    [Column(TypeName = "decimal(5,2)")]
    public decimal DeadTupleRatio { get; set; }
        
    public long TableSize { get; set; }
    public DateTime? LastVacuum { get; set; }
    public DateTime? LastAutoVacuum { get; set; }
        
    [Column(TypeName = "decimal(5,2)")]
    public decimal ChangeRatePercent { get; set; } // % изменения за период
    
    public Guid DbConnectionId { get; set; }
}