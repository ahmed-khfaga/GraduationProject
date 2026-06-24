using System.Security.Claims;
using FitZone.Service.DTOs.ChatDTOs;
using FitZone.Service.Errors;
using FitZone.Service.Services.Contract;
using FitZone.Service.Services.Contract.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.APIs.Controllers
{
    [Authorize(Roles = "Trainee,Coach")]
    public class ChatController : BaseApiController
    {
        private readonly IChatService _chatService;
        private readonly IMembershipService _membershipService;

        public ChatController(IChatService chatService, IMembershipService membershipService)
        {
            _chatService = chatService;
            _membershipService = membershipService;
        }

        [HttpGet("access/{otherUserId}")]
        public async Task<ActionResult<ChatAccessDto>> GetChatAccess(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            if (string.IsNullOrWhiteSpace(otherUserId))
                return BadRequest(new ApiException(400, "Other user is required."));

            if (userId == otherUserId)
                return BadRequest(new ApiException(400, "You cannot chat with yourself."));

            var canChat = await _chatService.CanUsersChatAsync(userId, otherUserId);
            if (!canChat)
            {
                return Ok(new ChatAccessDto
                {
                    CanChat = false,
                    HasPremiumAccess = false,
                    Message = "Chat is allowed only between assigned coach and trainee."
                });
            }

            if (User.IsInRole("Trainee"))
            {
                var hasPremiumAccess = await _membershipService.HasPremiumMembership(userId);
                if (!hasPremiumAccess)
                {
                    return Ok(new ChatAccessDto
                    {
                        CanChat = false,
                        HasPremiumAccess = false,
                        Message = "Upgrade to Premium to chat with your coach."
                    });
                }

                return Ok(new ChatAccessDto
                {
                    CanChat = true,
                    HasPremiumAccess = true
                });
            }

            return Ok(new ChatAccessDto
            {
                CanChat = true,
                HasPremiumAccess = true
            });
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetConversation(string otherUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new ApiException(401, "Invalid user token."));

            if (string.IsNullOrWhiteSpace(otherUserId))
                return BadRequest(new ApiException(400, "Other user is required."));

            if (userId == otherUserId)
                return BadRequest(new ApiException(400, "You cannot chat with yourself."));

            if (User.IsInRole("Trainee"))
            {
                var hasPremiumAccess = await _membershipService.HasPremiumMembership(userId);
                if (!hasPremiumAccess)
                    return StatusCode(403, new ApiException(403, "Upgrade to Premium to chat with your coach."));
            }

            var canChat = await _chatService.CanUsersChatAsync(userId, otherUserId);
            if (!canChat)
                return StatusCode(403, new ApiException(403, "Chat is allowed only between assigned coach and trainee."));

            var messages = await _chatService.GetConversation(userId, otherUserId);
            return Ok(messages);
        }
    }
}
