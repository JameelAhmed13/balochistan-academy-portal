using System.Text.Json;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.Infrastructure.Services;

public class AuditService(IApplicationDbContext db) : IAuditService
{
    public async Task LogAsync(int? userId, string action, string? entityType = null,
        int? entityId = null, object? oldValues = null, object? newValues = null,
        string? ip = null, CancellationToken ct = default)
    {
        var entry = new AuditLog
        {
            UserId     = userId,
            Action     = action,
            EntityType = entityType,
            EntityId   = entityId,
            OldValues  = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValues  = newValues is null ? null : JsonSerializer.Serialize(newValues),
            IpAddress  = ip,
            Timestamp  = DateTime.UtcNow,
        };
        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(ct);
    }
}
