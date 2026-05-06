using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Core.Entitys;
using FitZone.Service.DTOs;

namespace FitZone.Service.Services.Contract;

public interface IMembershipService
{
    Task<IEnumerable<MembershipWithPricePlanDto>> GetMembershipsByDurationAsync(int duration);
    Task<IEnumerable<MembershipPlansDto>> GetAllMembershipsPlan();
    Task<MembershipStatusDto> ActivateMembershipAsync(string applicationUserId, int membershipPlanId);
    Task<MembershipStatusDto> GetMyMembershipStatusAsync(string applicationUserId);

    Task<bool> HasPremiumMembership(string applicationUserId);

}
