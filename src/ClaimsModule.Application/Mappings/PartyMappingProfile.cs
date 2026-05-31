using AutoMapper;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Parties;

namespace ClaimsModule.Application.Mappings;

public sealed class PartyMappingProfile : Profile
{
    public PartyMappingProfile()
    {
        CreateMap<ClaimParty, ClaimPartyDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.PartyRole, o => o.MapFrom(s => s.PartyRole.ToString()))
            .ForMember(d => d.PartyType, o => o.MapFrom(s => s.PartyType.ToString()));
    }
}
