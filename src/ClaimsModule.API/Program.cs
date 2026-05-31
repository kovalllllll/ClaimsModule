using System.Text;
using System.Text.Json.Serialization;
using ClaimsModule.API;
using ClaimsModule.API.Auth;
using ClaimsModule.API.Middleware;
using ClaimsModule.API.Options;
using ClaimsModule.API.Services;
using ClaimsModule.Application;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Infrastructure;
using ClaimsModule.Infrastructure.Storage;
using ClaimsModule.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TenantOptions>(
    builder.Configuration.GetSection(TenantOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
builder.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddSingleton<MockJwtTokenService>();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSecret = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Claims Module API",
        Version = "v1",
        Description = "DICEUS Fullstack Assessment — FNOL & Reserve Management"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Mock JWT from POST /api/auth/token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

await app.ApplyMigrationsIfConfiguredAsync();

var disableHangfire = bool.TryParse(app.Configuration["Testing:DisableHangfire"], out var disableHangfireFlag)
    && disableHangfireFlag;
if (!disableHangfire)
{
    GlobalJobFilters.Filters.Add(
        new ClaimsModule.Infrastructure.Jobs.PostGlFailedStateFilter(
            app.Services.GetRequiredService<IServiceScopeFactory>()));
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims Module API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims Module API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyKeyMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var storageProvider = app.Configuration["StorageProvider"] ?? "LocalFileSystem";
if (storageProvider.Equals("LocalFileSystem", StringComparison.OrdinalIgnoreCase))
{
    var uploadsPath = LocalStoragePaths.ResolveBasePath(
        app.Configuration["LocalStorage:BasePath"],
        app.Environment.ContentRootPath);
    Directory.CreateDirectory(uploadsPath);
    app.UseMiddleware<LocalStorageAccessMiddleware>();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

app.MapControllers();

if (!disableHangfire)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = app.Environment.IsDevelopment()
            ? Array.Empty<IDashboardAuthorizationFilter>()
            : [new HangfireDashboardAuthorizationFilter()]
    });

    ClaimsModule.Infrastructure.DependencyInjection.ScheduleRecurringJobs(app.Configuration);
}

app.Run();

public partial class Program;
