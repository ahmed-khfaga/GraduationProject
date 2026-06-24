using System.Security.Claims;
using FitZone.Service.DTOs.PaymentDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    [Authorize(Roles = "Trainee")]
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("membership-intent/{membershipPlanId:int}")]
        public async Task<ActionResult<PaymentIntentDto>> CreateMembershipIntent(int membershipPlanId)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(appUserId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            var result = await _paymentService.CreateMembershipPaymentIntentAsync(appUserId, membershipPlanId);
            return Ok(result);
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentStatusDto>> Confirm([FromBody] ConfirmPaymentDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(appUserId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            if (string.IsNullOrWhiteSpace(dto.PaymentIntentId))
                return BadRequest(new ApiException(400, "PaymentIntentId is required."));

            if (string.IsNullOrWhiteSpace(dto.CardNumber))
                return BadRequest(new ApiException(400, "Card number is required."));

            var result = await _paymentService.ConfirmMembershipPaymentAsync(
                appUserId,
                dto.PaymentIntentId,
                dto.CardNumber);
            return Ok(result);
        }
    }
}
