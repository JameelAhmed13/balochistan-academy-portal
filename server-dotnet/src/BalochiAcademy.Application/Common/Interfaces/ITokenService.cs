using BalochiAcademy.Domain.Entities;

namespace BalochiAcademy.Application.Common.Interfaces;

public interface ITokenService
{
    string       GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(int userId);
    int?         ValidateAccessToken(string token);   // returns userId or null
}

public interface IPasswordService
{
    string Hash(string password);
    bool   Verify(string password, string hash);
}

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string? entityType = null,
                  int? entityId = null, object? oldValues = null,
                  object? newValues = null, string? ip = null,
                  CancellationToken ct = default);
}

public interface ICurrentUserService
{
    int?   UserId   { get; }
    string? Role    { get; }
    string? IpAddress { get; }
}
