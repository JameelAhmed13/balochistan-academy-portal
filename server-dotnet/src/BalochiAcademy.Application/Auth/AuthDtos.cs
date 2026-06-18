using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Auth;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record RegisterRequest(
    [Required, MinLength(1)] string Name,
    [Required, MinLength(3), MaxLength(40)] string Username,
    [Required, MinLength(4)] string Password,
    [Required] string GradeCode,
    string? Phone     = null,
    string? Email     = null,
    string? Medium    = "English",
    string? Institute = null
);

public record UpdateGradeRequest([Required] string GradeCode);

public record AuthResponse(string Token, string RefreshToken, UserDto User);

public record RefreshTokenRequest([Required] string RefreshToken);

public record UserDto(
    int     Id,
    string  Username,
    string  Role,
    string? Name,
    string? Phone,
    string? Email,
    string? GradeCode,
    string  Medium,
    string? Institute,
    string  Board,
    int     Coins
);
