using System.Linq.Expressions;
using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Documents;
using ClaimsModule.Domain.Parties;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Reserves;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence;

public sealed class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<LossEvent> LossEvents => Set<LossEvent>();
    public DbSet<ClaimParty> ClaimParties => Set<ClaimParty>();
    public DbSet<ClaimRiskObject> ClaimRiskObjects => Set<ClaimRiskObject>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimReserveComponent> ClaimReserveComponents => Set<ClaimReserveComponent>();
    public DbSet<ReserveHistory> ReserveHistory => Set<ReserveHistory>();
    public DbSet<ClaimAuditLog> ClaimAuditLog => Set<ClaimAuditLog>();
    public DbSet<CauseOfLossCode> CauseOfLossCodes => Set<CauseOfLossCode>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<ClaimSequence> ClaimSequences => Set<ClaimSequence>();
    public DbSet<ApiIdempotencyRecord> ApiIdempotencyRecords => Set<ApiIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeleted = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(
                Expression.Not(isDeleted),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
