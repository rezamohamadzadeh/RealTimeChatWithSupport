using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RealTimeChatWithSupport
{
    [Authorize]
    public class AgentHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly IHubContext<ChatHub> _chatHub;

        public AgentHub(
            IChatRoomService chatRoomService,
            IHubContext<ChatHub> chatHub)
        {
            _chatRoomService = chatRoomService;
            _chatHub = chatHub;
        }
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(
                "ActiveRooms",
                await _chatRoomService.GetAllRooms());

            await base.OnConnectedAsync();
        }
        [Authorize]
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Group(roomId).SendAsync(
                "ReceiveMessage",
                "Support",
                DateTimeOffset.UtcNow,
                "The user disconected !");
            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task GetRoomID(Guid roomId)
        {
            var id = Context.ConnectionId;

            var room = await _chatRoomService.GetRoom(roomId);
            await Clients.AllExcept(id).SendAsync(
                "GetRoomIdForFilterRooms",room.Id);
        }

        [Authorize]
        public async Task SendAgentMessage(Guid roomId, string text)
        {
            var message = new ChatMessage
            {
                SenderName = Context.User.Identity.Name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };

            await _chatRoomService.AddMessage(roomId, message);

            await _chatHub.Clients
                .Group(roomId.ToString())
                .SendAsync("ReceiveMessage",
                    message.SenderName,
                    message.SentAt,
                    message.Text);
            
        }
        [Authorize]
        public async Task LoadHistory(Guid roomId)
        {
            var history = await _chatRoomService
                .GetMessageHistory(roomId);

            await Clients.Caller.SendAsync(
                "ReceiveMessages", history);

            var claimsIdentity = (ClaimsIdentity)Context.User.Identity;
            if (!claimsIdentity.HasClaim(c => c.Type == "RoomId"))
            {
                claimsIdentity.AddClaim(new Claim("RoomId", roomId.ToString()));
            }
        }
        [Authorize]
        public async Task GetRoomIdForUpFile()
        {
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Caller.SendAsync("PassRoomId", roomId);
        }
    }
}
