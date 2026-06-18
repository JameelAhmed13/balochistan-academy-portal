using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Infrastructure.Persistence.Repositories;

namespace BalochiAcademy.Infrastructure.Persistence;

/// <summary>
/// Coordinates all repositories against a single ApplicationDbContext,
/// ensuring all operations within a request share one EF Core transaction.
/// </summary>
public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repos = new();

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repos.TryGetValue(type, out var repo))
        {
            repo = new Repository<T>(context);
            _repos[type] = repo;
        }
        return (IRepository<T>)repo;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);

    public void Dispose() => context.Dispose();
}
