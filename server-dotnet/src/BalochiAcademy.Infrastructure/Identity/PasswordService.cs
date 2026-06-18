using BalochiAcademy.Application.Common.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace BalochiAcademy.Infrastructure.Identity;

public class PasswordService : IPasswordService
{
    public string Hash(string password)   => BC.HashPassword(password, BC.GenerateSalt(12));
    public bool   Verify(string password, string hash) => BC.Verify(password, hash);
}
