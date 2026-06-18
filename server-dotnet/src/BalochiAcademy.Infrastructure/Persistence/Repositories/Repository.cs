using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.Infrastructure.Persistence.Repositories;

public sealed class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public IQueryable<T> Query()                          => _set;
    public void Add(T entity)                             => _set.Add(entity);
    public void AddRange(IEnumerable<T> entities)         => _set.AddRange(entities);
    public void Remove(T entity)                          => _set.Remove(entity);
    public void RemoveRange(IEnumerable<T> entities)      => _set.RemoveRange(entities);

    public ValueTask<T?> FindAsync(object[] keyValues, CancellationToken ct = default)
        => _set.FindAsync(keyValues, ct);
}
