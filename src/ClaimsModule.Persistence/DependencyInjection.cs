using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Persistence.Interceptors;
using ClaimsModule.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var skipDbRegistration = bool.TryParse(
            configuration["Testing:SkipDbRegistration"], out var skip) && skip;

        services.AddScoped<AuditingSaveChangesInterceptor>();
        services.AddScoped<SoftDeleteSaveChangesInterceptor>();
        services.AddScoped<AppendOnlySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        if (!skipDbRegistration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            var useInMemory = connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase);

            services.AddDbContext<ClaimsDbContext>((sp, options) =>
            {
                if (useInMemory)
                {
                    options.UseInMemoryDatabase("ClaimsModuleTests");
                }
                else
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ClaimsDbContext).Assembly.FullName);
                    });
                }

                options.AddInterceptors(
                    sp.GetRequiredService<AuditingSaveChangesInterceptor>(),
                    sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>(),
                    sp.GetRequiredService<AppendOnlySaveChangesInterceptor>(),
                    sp.GetRequiredService<DispatchDomainEventsInterceptor>());
            });
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClaimNumberGenerator, ClaimNumberGenerator>();
        services.AddScoped<IValidationQueries, ValidationQueries>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IReserveRepository, ReserveRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IApiIdempotencyRepository, ApiIdempotencyRepository>();

        return services;
    }
}
