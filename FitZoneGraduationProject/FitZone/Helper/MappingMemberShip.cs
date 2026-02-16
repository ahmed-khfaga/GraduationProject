using AutoMapper;
using FitZone.Service.DTOs;
using FitZone.Core.Entitys;

namespace FitZone.APIs.Helper
{
    public class MappingMemberShip : Profile
    {

        public MappingMemberShip()
        {
            CreateMap<MembershipPlan, MembershipWithPricePlanDTOs>()
                .ForMember(D => D.Name, O => O.MapFrom(S => S.Membership.Name))
                .ForMember(D => D.Description, O => O.MapFrom(S => S.Membership.Description));

            CreateMap<MembershipPlan, MembershipPlansDTOs>()
                .ForMember(D => D.Name, O => O.MapFrom(S => S.Membership.Name));               
        }
    }
}
