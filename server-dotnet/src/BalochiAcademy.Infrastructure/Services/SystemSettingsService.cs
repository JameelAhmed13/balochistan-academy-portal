using System.Collections.Concurrent;
using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BalochiAcademy.Infrastructure.Services;

public sealed class SystemSettingsService(IServiceScopeFactory scopeFactory) : ISystemSettingsService
{
    private ConcurrentDictionary<string, string?> _cache = new();
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private async Task EnsureCacheAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _cacheExpiry) return;
        await _lock.WaitAsync(ct);
        try
        {
            if (DateTime.UtcNow < _cacheExpiry) return;
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var all = await db.SystemSettings.AsNoTracking().ToListAsync(ct);
            _cache = new ConcurrentDictionary<string, string?>(
                all.ToDictionary(s => s.Key, s => s.Value));
            _cacheExpiry = DateTime.UtcNow.AddMinutes(5);
        }
        finally { _lock.Release(); }
    }

    public async Task<string?> GetAsync(string key, string? fallback = null, CancellationToken ct = default)
    {
        await EnsureCacheAsync(ct);
        return _cache.TryGetValue(key, out var val) ? val ?? fallback : fallback;
    }

    public async Task SetAsync(string key, string value, int? updatedById = null, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var setting = await db.SystemSettings.FindAsync(new object[] { key }, ct);
        if (setting != null)
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedById = updatedById;
            await db.SaveChangesAsync(ct);
        }
        _cache[key] = value;
    }

    public void Invalidate() => _cacheExpiry = DateTime.MinValue;
}
