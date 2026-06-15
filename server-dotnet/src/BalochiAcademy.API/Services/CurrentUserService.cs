using System.Security.Claims;
using BalochiAcademy.Application.Common.Interfaces;

namespace BalochiAcademy.API.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public int? UserId
    {
        get
        {
            var v = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }
    }

    public string? Role
        => accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    public string? IpAddress
        => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
