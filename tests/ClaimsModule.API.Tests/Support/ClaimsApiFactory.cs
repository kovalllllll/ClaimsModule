using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClaimsModule.API.Tests.Support;

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      services.AddDbContext<ClaimsDbContext>(options =>
      {
        options.UseInMemoryDatabase("ClaimsModuleIntegrationTests")
          .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
      });

      services.RemoveAll<IClaimNumberGenerator>();
      services.AddSingleton<IClaimNumberGenerator, TestClaimNumberGenerator>();
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
