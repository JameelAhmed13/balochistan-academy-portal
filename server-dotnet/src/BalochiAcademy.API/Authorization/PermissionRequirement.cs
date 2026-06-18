using Microsoft.AspNetCore.Authorization;

namespace BalochiAcademy.API.Authorization;

/// <summary>Authorization requirement for a specific granular permission.</summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

/// <summary>
/// Dynamic policy provider that converts "Permission:{name}" policy names
/// into PermissionRequirement instances at runtime.
/// This allows [Authorize(Policy = "Permission:questions.manage")] without
/// registering every permission combination at startup.
/// </summary>
public sealed class PermissionPolicyProvider(IAuthorizationPolicyProvider fallback)
    : IAuthorizationPolicyProvider
{
    internal const string PolicyPrefix = "Permission:";

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
        return fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()     => fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()   => fallback.GetFallbackPolicyAsync();
}
