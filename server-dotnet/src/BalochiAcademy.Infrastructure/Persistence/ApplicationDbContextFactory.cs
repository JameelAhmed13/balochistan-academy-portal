using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BalochiAcademy.Infrastructure.Persistence;

/// <summary>
/// Used by 'dotnet ef migrations add/update' at design time only — not used at runtime.
/// Override BALOCHI_CONNSTR env var to point at a different server.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("BALOCHI_CONNSTR")
            ?? "Server=localhost\\SQLEXPRESS;Database=BalochiAcademyDB;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connStr)
            .Options;

        return new ApplicationDbContext(options);
    }
}
