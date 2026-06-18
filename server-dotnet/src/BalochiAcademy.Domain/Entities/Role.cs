using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Role : AuditableEntity
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<User>           Users            { get; set; } = [];
}

public class Permission : AuditableEntity
{
    public string  Name        { get; set; } = string.Empty;
    public string  Module      { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

public class RolePermission
{
    public int RoleId       { get; set; }
    public int PermissionId { get; set; }
    public Role       Role       { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
