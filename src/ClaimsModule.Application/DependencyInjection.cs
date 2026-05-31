using ClaimsModule.Application.Abstractions.Messaging;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Claims.Services;
using ClaimsModule.Application.Common.Behaviours;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Application.Common.Services;
using ClaimsModule.Application.Reserves;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClaimsModule.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehaviour<,>));
        });

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddScoped<IValidationWarningCollector, ValidationWarningCollector>();
        services.AddScoped<IClaimClosureEvaluator, ClaimClosureEvaluator>();
        services.AddScoped<ReserveApiIdempotency>();

        return services;
    }
}
