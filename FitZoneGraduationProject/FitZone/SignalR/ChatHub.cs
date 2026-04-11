using System.Security.Claims;
using FitZone.Service.Services.Contract;
using FitZone.Service.Services.Contract.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FitZone.APIs.SignalR
{
    [Authorize(Roles ="Trainee,Coach")]
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

            // (optional) log or track
            Console.WriteLine($"User connected: {userId}");

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var hasAccess = await _membershipService.HasPremiumMembership(senderId);

            if (!hasAccess)
                throw new HubException("Upgrade to Premium to chat");

            // ✅ Save message
            await _chatService.SaveMessageAsync(senderId, receiverId, message);

            // ✅ Send to receiver
            await Clients.User(receiverId)
                .SendAsync("ReceiveMessage", senderId, message);

            // ✅ (Optional) Send back to sender
            await Clients.Caller
                .SendAsync("ReceiveMessage", senderId, message);
        }
    }
}
