using AutoMapper;
using GymFit.Application.DTOs.Messages;
using GymFit.Application.DTOs.Plans;
using GymFit.Application.DTOs.Trainers;
using GymFit.Application.DTOs.Users;
using GymFit.Domain.Entities;

namespace GymFit.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.HasTrainerProfile, o => o.MapFrom(s => s.TrainerProfile != null));

        CreateMap<Plan, PlanDto>();
        CreateMap<Message, MessageDto>();

        CreateMap<TrainerOrder, TrainerOrderDto>()
            .ForMember(d => d.ClientUserId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.ClientFullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.TrainerUserId, o => o.MapFrom(s => s.TrainerId))
            .ForMember(d => d.TrainerFullName, o => o.MapFrom(s => s.Trainer.FullName));
    }
}
