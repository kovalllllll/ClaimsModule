using AutoMapper;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Mappings;

public sealed class ClaimMappingProfile : Profile
{
    public ClaimMappingProfile()
    {
        // ClaimDetailDto is constructed manually in GetClaimDetailQueryHandler
        // to support SAS URL generation, reserve history loading, and
        // computed fields (ValidNextStatuses, RecentAuditEntries). No profile needed.

        // Used by ListClaimsQueryHandler (manual projection) — no AutoMapper map needed
        // for ClaimSummaryDto either, but LossEvent and RiskObject sub-maps are shared.

        CreateMap<LossEvent, LossEventDto>()
            .ForMember(d => d.EstimatedLossAmount,
                o => o.MapFrom(s => s.EstimatedLossAmount != null
                    ? (decimal?)s.EstimatedLossAmount.Amount
                    : null));

        CreateMap<ClaimRiskObject, ClaimRiskObjectDto>()
            .ForMember(d => d.AssetType, o => o.MapFrom(s => s.AssetType.ToString()));
    }
}
