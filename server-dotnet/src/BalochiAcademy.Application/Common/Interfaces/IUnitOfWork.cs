namespace BalochiAcademy.Application.Common.Interfaces;

/// <summary>
/// Unit of Work: coordinates multiple repositories under a single transaction.
/// Repositories are created lazily and cached per request scope.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Returns the repository for entity type T.</summary>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>Persist all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
