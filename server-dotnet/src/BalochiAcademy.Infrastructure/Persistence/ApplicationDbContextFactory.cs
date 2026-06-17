using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BalochiAcademy.Infrastructure.Persistence;

/// <summary>
/// Used by 'dotnet ef migrations add/update' at design time only — not used at runtime.
///
/// Connection string resolution order (first match wins):
///   1. Environment variable  ConnectionStrings__Default
///   2. appsettings.{ASPNETCORE_ENVIRONMENT}.json  (e.g. appsettings.Development.json)
///   3. appsettings.json
///
/// Override example (PowerShell):
///   $env:ConnectionStrings__Default = "Server=..."; dotnet ef database update ...
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",                    optional: true)
            .AddJsonFile($"appsettings.{env}.json",             optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found. " +
                "Set it in appsettings.json or via the " +
                "ConnectionStrings__Default environment variable.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connStr)
            .Options;

        return new ApplicationDbContext(options);
    }
}
