using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BalochiAcademy.API.Authorization;

/// <summary>
/// Resolves PermissionRequirement by checking whether the authenticated user's
/// role has the required permission in the RolePermissions table.
/// Admins bypass all permission checks (they have unrestricted access by convention).
/// </summary>
public sealed class PermissionAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement       requirement)
    {
        // Admins bypass granular permission checks
        if (context.User.IsInRole("admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdClaim = context.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return;

        var hasPermission = await permissionService.HasPermissionAsync(userId, requirement.Permission);
        if (hasPermission)
            context.Succeed(requirement);
    }
}
