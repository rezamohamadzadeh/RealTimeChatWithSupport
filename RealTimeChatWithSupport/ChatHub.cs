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
        /// <summary>
        /// Method of disconnecting the user
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
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
                DateTime.Now,
                "The user disconected !");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Method of sconnecting the user
        /// </summary>
        /// <returns></returns>
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
                DateTime.Now,
                "How can i help you?");

            await base.OnConnectedAsync();
        }
        /// <summary>
        /// This function is used to send a user message to support
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [Authorize]
        public async Task SendMessage(string name, string text)
        {
            var room = await _chatRoomService.GetRoomForConnectionId(
                Context.ConnectionId);

            var message = new ChatMessage
            {
                SenderName = name,
                Text = text
            };

            await _chatRoomService.AddMessage(room.Id, message);

            // Broadcast to all clients
            await Clients.Group(room.Id.ToString()).SendAsync(
                "ReceiveMessage",
                message.SenderName,
                message.Text);            
        }
        /// <summary>
        ///(Create group by userName) This function is executed when the user selects the connection button by connecting
        /// to the support and a new group is created based on the user name.
        /// </summary>
        /// <param name="visitorName"></param>
        /// <returns></returns>

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
        /// <summary>
        /// This function is used to send the group code to the view and save it in the hidden input tag
        /// to send the file
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task GetRoomId()
        {
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Caller.SendAsync("PassRoomId", roomId);
        }

        /// <summary>
        /// This function is called when the user clicks on the title of the group
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// This function is called when the user switch on the other group
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task LeaveRoom(Guid roomId)
        {
            if (roomId == Guid.Empty)
                throw new ArgumentException("Invalid Room ID");

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, roomId.ToString());
        }

        /// <summary>
        /// Initialize InitSurveyForm for user if admin dont answer after 2 min!
        /// </summary>
        /// <returns></returns>

        [Authorize]
        public async Task InitSurveyForm(Guid roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("RunTimeOut");
        }
    }
}
