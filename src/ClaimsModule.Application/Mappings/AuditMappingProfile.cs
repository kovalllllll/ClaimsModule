using AutoMapper;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Audit;

namespace ClaimsModule.Application.Mappings;

public sealed class AuditMappingProfile : Profile
{
    public AuditMappingProfile()
    {
        CreateMap<ClaimAuditLog, AuditLogEntryDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.EventType, o => o.MapFrom(s => AuditEventTypeFormatter.ToSpecificationString(s.EventType)));
    }
}
