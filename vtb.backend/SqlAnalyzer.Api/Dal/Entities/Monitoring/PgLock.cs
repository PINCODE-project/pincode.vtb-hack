using SqlAnalyzer.Api.Dal.Entities.Base;

namespace SqlAnalyzer.Api.Dal.Entities.Monitoring;

/// <summary>
/// Модель для хранения информации о заблокированных lock'ах PostgreSQL
/// </summary>
public class PgLock : EntityBase, IEntityCreatedAt
{
    /// <summary>
    /// Время сбора метрики
    /// </summary>
    public DateTime CreateAt { get; set; }

    /// <summary>
    /// Тип блокировки (relation, tuple, transactionid, etc.)
    /// </summary>
    public string LockType { get; set; } = string.Empty;

    /// <summary>
    /// OID базы данных, к которой относится блокировка
    /// </summary>
    public uint DatabaseOid { get; set; }

    /// <summary>
    /// OID отношения (таблицы), к которой относится блокировка
    /// </summary>
    public uint? RelationOid { get; set; }

    /// <summary>
    /// Номер страницы в отношении (для tuple locks)
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Номер tuple в странице (для tuple locks)
    /// </summary>
    public int? Tuple { get; set; }

    /// <summary>
    /// Виртуальный ID транзакции
    /// </summary>
    public string? VirtualXid { get; set; }

    /// <summary>
    /// ID транзакции
    /// </summary>
    public uint? TransactionId { get; set; }

    /// <summary>
    /// OID системного класса
    /// </summary>
    public uint? ClassId { get; set; }

    /// <summary>
    /// OID объекта внутри класса
    /// </summary>
    public uint? ObjectId { get; set; }

    /// <summary>
    /// Номер под-object'а
    /// </summary>
    public int? ObjectSubId { get; set; }

    /// <summary>
    /// Виртуальная транзакция backend'а
    /// </summary>
    public string VirtualTransaction { get; set; } = string.Empty;

    /// <summary>
    /// Process ID backend'а, удерживающего блокировку
    /// </summary>
    public int Pid { get; set; }

    /// <summary>
    /// Флаг, указывающий получена ли блокировка
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// Режим блокировки (ShareLock, ExclusiveLock, etc.)
    /// </summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>
    /// Флаг быстрого пути блокировки
    /// </summary>
    public bool FastPath { get; set; }

    /// <summary>
    /// Текущий запрос backend'а
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Имя приложения, выполняющего запрос
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Имя пользователя, выполняющего запрос
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Время начала выполнения запроса
    /// </summary>
    public DateTime? QueryStart { get; set; }

    /// <summary>
    /// Состояние backend'а (active, idle, etc.)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Время ожидания блокировки (в миллисекундах)
    /// </summary>
    public long? WaitTimeMs { get; set; }
    
    /// <summary>
    /// Идентификатор подключения
    /// </summary>
    public Guid DbConnectionId { get; set; }
}