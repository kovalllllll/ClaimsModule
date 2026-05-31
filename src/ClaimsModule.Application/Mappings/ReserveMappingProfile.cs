using AutoMapper;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Application.Mappings;

public sealed class ReserveMappingProfile : Profile
{
    public ReserveMappingProfile()
    {
        CreateMap<ClaimReserveComponent, ReserveComponentSummaryDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.ComponentType, o => o.MapFrom(s => s.Component.ToString()))
            .ForMember(d => d.CurrentAmount, o => o.MapFrom(s => s.CurrentAmount.Amount))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<ReserveHistory, ReserveTransactionDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.PreviousBalance, o => o.MapFrom(s => s.PreviousBalance.Amount))
            .ForMember(d => d.NewBalance, o => o.MapFrom(s => s.NewBalance.Amount))
            .ForMember(d => d.TransactionType, o => o.MapFrom(s => s.TransactionType.ToString()))
            .ForMember(d => d.ApprovalStatus, o => o.MapFrom(s => s.ApprovalStatus.ToString()))
            .ForMember(d => d.PostingStatus, o => o.MapFrom(s => s.PostingStatus.ToString()))
            .ForMember(d => d.IdempotencyKey, o => o.MapFrom(s => s.IdempotencyKey.Value));
    }
}
