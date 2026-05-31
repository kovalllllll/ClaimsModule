using System.Text.Json;
using AutoMapper;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Application.Mappings;

public sealed class PolicyMappingProfile : Profile
{
    public PolicyMappingProfile()
    {
        CreateMap<Policy, PolicySummaryDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CoverageTypes, o => o.MapFrom(s =>
                JsonSerializer.Deserialize<List<string>>(s.CoverageTypes, (JsonSerializerOptions?)null) ?? new List<string>()));

        CreateMap<CauseOfLossCode, CauseOfLossCodeDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.PerilCategory, o => o.MapFrom(s => s.PerilCategory.ToString()));
    }
}
