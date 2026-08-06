using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Erp.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var envPath = Path.GetFullPath(Path.Combine(
            Directory.GetCurrentDirectory(), "..", "Erp.Api", ".env"));
        if (File.Exists(envPath)) DotNetEnv.Env.Load(envPath);

        var cs = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                 ?? "Server=localhost;Database=erp_modular;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cs, sql => sql.MigrationsHistoryTable("__ef_migrations_history", "erp_sys"))
            .Options;

        return new AppDbContext(options);
    }
}
