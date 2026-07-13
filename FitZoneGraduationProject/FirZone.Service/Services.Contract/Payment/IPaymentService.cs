using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FitZone.Service.DTOs.PaymentDTOs;

namespace FitZone.Service.Services.Contract.Payment
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreateMembershipPaymentIntentAsync(string userId, int membershipPlanId);
        Task<PaymentStatusDto> ConfirmMembershipPaymentAsync(string userId, string paymentIntentId);
        Task<bool> HasSuccessfulPaymentForPlanAsync(string userId, int membershipPlanId, string paymentIntentId);
    }
}
