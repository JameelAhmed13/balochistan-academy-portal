using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "AdminOnly")]
public class RolesController(IUnitOfWork uow) : ControllerBase
{
    /// <summary>GET /api/admin/roles — all roles with their assigned permission IDs and user count</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var roles = await uow.Repository<Role>().Query()
            .Include(r => r.RolePermissions)
            .Include(r => r.Users)
            .OrderBy(r => r.Id)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.CreatedAt,
                UserCount = r.Users.Count,
                PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList(),
            })
            .ToListAsync(ct);

        return Ok(roles);
    }

    /// <summary>POST /api/admin/roles — create a new role</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Role name is required." });

        var exists = await uow.Repository<Role>().Query()
            .AnyAsync(r => r.Name == req.Name.Trim().ToLower(), ct);
        if (exists)
            return Conflict(new { message = "A role with this name already exists." });

        var role = new Role
        {
            Name        = req.Name.Trim().ToLower(),
            Description = req.Description?.Trim(),
        };
        uow.Repository<Role>().Add(role);
        await uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(List), new { id = role.Id }, new
        {
            role.Id, role.Name, role.Description, role.CreatedAt,
            UserCount = 0, PermissionIds = Array.Empty<int>(),
        });
    }

    /// <summary>PUT /api/admin/roles/{id} — update role name/description</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RoleRequest req, CancellationToken ct)
    {
        var role = await uow.Repository<Role>().FindAsync([id], ct);
        if (role == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Name))
            role.Name = req.Name.Trim().ToLower();
        role.Description = req.Description?.Trim();

        await uow.SaveChangesAsync(ct);
        return Ok(new { role.Id, role.Name, role.Description });
    }

    /// <summary>DELETE /api/admin/roles/{id} — delete role (409 if users are assigned)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var hasUsers = await uow.Repository<User>().Query()
            .AnyAsync(u => u.RoleId == id, ct);
        if (hasUsers)
            return Conflict(new { message = "Cannot delete a role that has users assigned to it." });

        var role = await uow.Repository<Role>().FindAsync([id], ct);
        if (role == null) return NotFound();

        uow.Repository<Role>().Remove(role);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>PUT /api/admin/roles/{id}/permissions — replace the permission set for a role</summary>
    [HttpPut("{id:int}/permissions")]
    public async Task<IActionResult> UpdatePermissions(int id, [FromBody] int[] permissionIds, CancellationToken ct)
    {
        var role = await uow.Repository<Role>().Query()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role == null) return NotFound();

        // Remove existing assignments
        foreach (var rp in role.RolePermissions.ToList())
            uow.Repository<RolePermission>().Remove(rp);

        // Add new assignments
        foreach (var pid in permissionIds.Distinct())
            uow.Repository<RolePermission>().Add(new RolePermission { RoleId = id, PermissionId = pid });

        await uow.SaveChangesAsync(ct);
        return Ok(new { roleId = id, permissionIds = permissionIds.Distinct().ToArray() });
    }

    /// <summary>GET /api/admin/permissions — seed-managed list of all permissions (read-only)</summary>
    [HttpGet("/api/admin/permissions")]
    public async Task<IActionResult> ListPermissions(CancellationToken ct)
    {
        var permissions = await uow.Repository<Permission>().Query()
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Module, p.Description })
            .ToListAsync(ct);
        return Ok(permissions);
    }
}

public record RoleRequest(string Name, string? Description = null);
