namespace BalochiAcademy.Application.Common.Interfaces;

/// <summary>Checks granular permissions stored in the RolePermissions table.</summary>
public interface IPermissionService
{
    /// <summary>Returns true if the user's role has the given permission name.</summary>
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default);

    /// <summary>Returns all permission names granted to the user's role.</summary>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);
}
