using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BalochiAcademy.Infrastructure.Services;

/// <summary>
/// Loads a user's permissions from RolePermissions and caches them for 5 minutes.
/// Cache is keyed per user so role changes propagate within one cache TTL.
/// </summary>
public sealed class PermissionService(IApplicationDbContext db, IMemoryCache cache) : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default)
    {
        var perms = await GetUserPermissionsAsync(userId, ct);
        return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default)
    {
        var cacheKey = $"perms:{userId}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached))
            return cached!;

        var roleId = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.RoleId)
            .FirstOrDefaultAsync(ct);

        List<string> permissions;
        if (roleId == null)
        {
            permissions = [];
        }
        else
        {
            permissions = await db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Name)
                .ToListAsync(ct);
        }

        var result = permissions.AsReadOnly();
        cache.Set(cacheKey, result, CacheTtl);
        return result;
    }
}
