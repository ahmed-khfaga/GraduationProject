using System.Security.Claims;
using FitZone.Service.DTOs.ChatDTOs;
using FitZone.Service.Services.Contract;
using FitZone.Service.Services.Contract.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FitZone.APIs.SignalR
{
    [Authorize(Roles = "Trainee,Coach")]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IMembershipService _membershipService;

        public ChatHub(IChatService chatService, IMembershipService membershipService)
        {
            _chatService = chatService;
            _membershipService = membershipService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User connected: {userId}");
            await base.OnConnectedAsync();
        }

        // ── Human↔Human (original, unchanged) ───────────────────────────────────

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(senderId))
                throw new HubException("Unauthorized user.");

            if (string.IsNullOrWhiteSpace(receiverId))
                throw new HubException("Receiver is required.");

            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty.");

            if (senderId == receiverId)
                throw new HubException("You cannot message yourself.");

            var senderIsTrainee = Context.User.IsInRole("Trainee");
            if (senderIsTrainee)
            {
                var hasPremiumAccess = await _membershipService.HasPremiumMembership(senderId);
                if (!hasPremiumAccess)
                    throw new HubException("Upgrade to Premium to chat");
            }

            var canChat = await _chatService.CanUsersChatAsync(senderId, receiverId);
            if (!canChat)
                throw new HubException("Chat is allowed only between assigned coach and trainee.");

            var normalizedMessage = message.Trim();
            await _chatService.SaveMessageAsync(senderId, receiverId, normalizedMessage);

            await Clients.User(receiverId)
                .SendAsync("ReceiveMessage", senderId, normalizedMessage);

            await Clients.Caller
                .SendAsync("ReceiveMessage", senderId, normalizedMessage);
        }

        // ── Bot chat ─────────────────────────────────────────────────────────────

        public async Task SendBotMessage(string conversationId, string message)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            if (string.IsNullOrWhiteSpace(conversationId) || !Guid.TryParse(conversationId, out var conversationGuid))
                throw new HubException("Valid conversationId (Guid) is required.");

            if (string.IsNullOrWhiteSpace(message))
                throw new HubException("Message cannot be empty.");

            var normalizedMessage = message.Trim();

          
            await _chatService.SaveBotTurnAsync(userId, conversationGuid, "user", normalizedMessage);

          
            await Clients.Caller.SendAsync("ReceiveBotMessage", "user", normalizedMessage, DateTime.UtcNow);

   
            var history = await _chatService.GetBotConversationAsync(userId, conversationGuid);

            
            var reply = "TODO: wire your OpenAI service here";

         
            await _chatService.SaveBotTurnAsync(userId, conversationGuid, "assistant", reply);

            
            await Clients.Caller.SendAsync("ReceiveBotMessage", "assistant", reply, DateTime.UtcNow);
        }

        
        public async Task GetBotHistory()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            var history = await _chatService.GetBotHistoryAsync(userId);

            await Clients.Caller.SendAsync("ReceiveBotHistory", history);
        }

       
        public async Task GetBotConversation(string conversationId)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized user.");

            if (string.IsNullOrWhiteSpace(conversationId) || !Guid.TryParse(conversationId, out var conversationGuid))
                throw new HubException("Valid conversationId (Guid) is required.");

            var messages = await _chatService.GetBotConversationAsync(userId, conversationGuid);

            await Clients.Caller.SendAsync("ReceiveBotConversation", messages);
        }
    }
}