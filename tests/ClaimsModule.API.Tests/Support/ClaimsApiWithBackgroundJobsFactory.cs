using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Infrastructure.Jobs;
using ClaimsModule.Persistence;
using ClaimsModule.Persistence.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClaimsModule.API.Tests.Support;

public sealed class ClaimsApiWithBackgroundJobsFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public MutableSystemClock Clock { get; } = new();

    public ConfigurableGlPostingSimulator GlSimulator { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<ClaimsDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase("ClaimsModuleBackgroundJobTests")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                options.AddInterceptors(
                    sp.GetRequiredService<AuditingSaveChangesInterceptor>(),
                    sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>(),
                    sp.GetRequiredService<AppendOnlySaveChangesInterceptor>(),
                    sp.GetRequiredService<DispatchDomainEventsInterceptor>());
            });

            services.RemoveAll<IClaimNumberGenerator>();
            services.AddSingleton<IClaimNumberGenerator, TestClaimNumberGenerator>();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISystemClock>();
            services.AddSingleton(Clock);
            services.AddSingleton<ISystemClock>(sp => sp.GetRequiredService<MutableSystemClock>());

            services.RemoveAll<IGlPostingSimulator>();
            services.AddSingleton(GlSimulator);
            services.AddSingleton<IGlPostingSimulator>(sp => sp.GetRequiredService<ConfigurableGlPostingSimulator>());

            services.RemoveAll<IJobScheduler>();
            services.AddScoped<IJobScheduler, SynchronousJobScheduler>();
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
