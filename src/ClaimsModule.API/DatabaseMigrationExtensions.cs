using ClaimsModule.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.API;

internal static class DatabaseMigrationExtensions
{
    public static async Task ApplyMigrationsIfConfiguredAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        await db.Database.MigrateAsync();
    }
}
