using Azure.Storage.Blobs;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Infrastructure.Jobs;
using ClaimsModule.Infrastructure.Persistence;
using ClaimsModule.Infrastructure.Storage;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuditLogService, AuditLogService>();

        var storageProvider = configuration["StorageProvider"] ?? "LocalFileSystem";
        if (storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = configuration["AzureBlob:ConnectionString"]!;
            var containerName = configuration["AzureBlob:ContainerName"] ?? "claim-documents";
            services.AddSingleton(_ => new BlobServiceClient(connectionString));
            services.AddSingleton<IStorageService>(sp =>
                new AzureBlobStorageService(sp.GetRequiredService<BlobServiceClient>(), containerName));
        }
        else
        {
            var baseUrl = configuration["LocalStorage:BaseUrl"] ?? "/uploads";
            var signingKey = configuration["LocalStorage:SigningKey"]
                ?? "ClaimsModuleLocalStorageSigningKeyDevOnly2026!";
            services.AddSingleton<IStorageService>(sp =>
            {
                var contentRoot = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>().ContentRootPath;
                var basePath = LocalStoragePaths.ResolveBasePath(
                    configuration["LocalStorage:BasePath"], contentRoot);
                return new LocalFileSystemStorageService(basePath, baseUrl, signingKey);
            });
        }

        var disableHangfire = bool.TryParse(
            configuration["Testing:DisableHangfire"], out var disable) && disable;
        if (!disableHangfire)
        {
            var hangfireConnectionString = configuration.GetConnectionString("DefaultConnection")!;
            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();
        }

        services.AddScoped<IGlPostingSimulator, DefaultGlPostingSimulator>();
        services.AddScoped<PostGLReserveChangeJob>();
        services.AddScoped<SlaMonitoringJob>();

        if (disableHangfire)
            services.AddScoped<IJobScheduler, NoOpJobScheduler>();
        else
            services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    public static void ScheduleRecurringJobs(IConfiguration? configuration = null)
    {
        if (bool.TryParse(configuration?["Testing:DisableHangfire"], out var disable) && disable)
            return;

        RecurringJob.AddOrUpdate<SlaMonitoringJob>(
            "sla-monitoring",
            job => job.ExecuteAsync(CancellationToken.None),
            "*/15 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}
