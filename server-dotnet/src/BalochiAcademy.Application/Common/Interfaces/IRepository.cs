namespace BalochiAcademy.Application.Common.Interfaces;

/// <summary>Generic repository abstraction for a single entity type.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>Returns an IQueryable for complex queries (Where, Include, OrderBy, etc.).</summary>
    IQueryable<T> Query();

    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>Find by primary key(s). Returns null if not found.</summary>
    ValueTask<T?> FindAsync(object[] keyValues, CancellationToken ct = default);
}
