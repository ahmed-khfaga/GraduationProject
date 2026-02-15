using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitZone.Service.DTOs;

namespace FitZone.Service.Services.Contract
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipWithPricePlanDTOs>> GetMembershipsByDurationAsync(int duration);

    }
}
