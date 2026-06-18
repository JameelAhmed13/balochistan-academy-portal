using Microsoft.AspNetCore.Authorization;

namespace BalochiAcademy.API.Authorization;

/// <summary>
/// Shorthand for [Authorize(Policy = "Permission:{name}")].
/// Usage: [RequirePermission("reports.view")]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permission)
    : AuthorizeAttribute($"{PermissionPolicyProvider.PolicyPrefix}{permission}");
