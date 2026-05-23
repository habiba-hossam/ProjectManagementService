using AutoMapper;
using ProjectManagementAPI.Core.Application.Features.Auth.Commands;
using ProjectManagementAPI.Core.Application.Features.Auth.Commands.Register;
using ProjectManagementAPI.Core.Application.Features.Projects;
using ProjectManagementAPI.Core.Application.Features.Tasks;
using ProjectManagementAPI.Core.Domain.Entities;

namespace ProjectManagementAPI.Core.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── User → AuthResponseDto ────────────────────────────────────────────
        CreateMap<User, AuthResponseDto>()
            
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Token, opt => opt.Ignore()); // Token is generated separately, so we ignore it here

            

        // ── Project → ProjectDto ──────────────────────────────────────────────
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));

        // ── ProjectTask → TaskDto ─────────────────────────────────────────────
        CreateMap<ProjectTask, TaskDto>()
            .ForMember(dest => dest.StatusLabel,   opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PriorityLabel, opt => opt.MapFrom(src => src.Priority.ToString()));
    }
}