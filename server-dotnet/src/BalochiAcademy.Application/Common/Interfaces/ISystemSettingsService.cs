namespace BalochiAcademy.Application.Common.Interfaces;

public interface ISystemSettingsService
{
    Task<string?> GetAsync(string key, string? fallback = null, CancellationToken ct = default);
    Task SetAsync(string key, string value, int? updatedById = null, CancellationToken ct = default);
    void Invalidate();
}
