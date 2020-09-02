using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RealTimeChatWithSupport
{
    public class ChatHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly IHubContext<AgentHub> _agentHub;
        private IServiceProvider _sp;

        public IHttpContextAccessor _httpContext { get; }

        public ChatHub(
            IChatRoomService chatRoomService,
            IHubContext<AgentHub> agentHub,
            IServiceProvider sp,
            IHttpContextAccessor httpContext)
        {
            _chatRoomService = chatRoomService;
            _agentHub = agentHub;
            _sp = sp;
            _httpContext = httpContext;
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var role = _httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role == "Admin")
            {
                // Authenticated agents don't need a room
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var roomId = _httpContext.HttpContext.User.FindFirstValue("RoomId");
            await Clients.Group(roomId).SendAsync(
                "ReceiveMessage",
                "Support",
                DateTimeOffset.UtcNow,
                "The user disconected !");
            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var role = _httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            if (role == "Admin")
            {
                // Authenticated agents don't need a room
                await base.OnConnectedAsync();
                return;
            }

            var roomId = await _chatRoomService.CreateRoom(
                Context.ConnectionId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync(
                "ReceiveMessage",
                "Hello",
                DateTimeOffset.UtcNow,
                "How can i help you?");

            await base.OnConnectedAsync();
        }
        [Authorize]
        public async Task SendMessage(string name, string text)
        {
            var room = await _chatRoomService.GetRoomForConnectionId(
                Context.ConnectionId);

            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };

            await _chatRoomService.AddMessage(room.Id, message);

            // Broadcast to all clients
            await Clients.Group(room.Id.ToString()).SendAsync(
                "ReceiveMessage",
                message.SenderName,
                message.SentAt,
                message.Text);

        }
        [Authorize]
        public async Task SetName(string visitorName)
        {
            var roomName = $"For chat with {visitorName} click here";

            var room = await _chatRoomService.GetRoomForConnectionId(
                Context.ConnectionId);

            room = await _chatRoomService.SetRoomName(room.Id, roomName);
            
            var claimsIdentity = (ClaimsIdentity)_httpContext.HttpContext.User.Identity;

            if (!claimsIdentity.HasClaim(c => c.Type == "RoomId"))
            {
                claimsIdentity.AddClaim(new Claim("RoomId", room.Id.ToString()));

            }
            await _agentHub.Clients.All.SendAsync("SetNewRoom", room);
        }
        [Authorize]
        public async Task GetRoomId()
        {
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Caller.SendAsync("PassRoomId", roomId);
        }

        [Authorize]
        public async Task JoinRoom(Guid roomId)
        {
            try
            {
                if (roomId == Guid.Empty)
                    throw new ArgumentException("Invalid Room ID");

                var userId = _httpContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var room = dbContext.ChatRooms.FirstOrDefault(d => d.Id == roomId);
                    if (room != null)
                    {
                        if (room.UserId == null)
                        {
                            room.UserId = userId;
                            dbContext.SaveChanges();
                        }
                        else if (userId != room.UserId) return;

                    }

                }

                await Groups.AddToGroupAsync(
                    Context.ConnectionId, roomId.ToString());
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [Authorize]
        public async Task LeaveRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Invalid Room ID");

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, roomId.ToString());
        }
    }
}
