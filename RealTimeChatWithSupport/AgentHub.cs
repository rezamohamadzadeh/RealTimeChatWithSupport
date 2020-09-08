using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RealTimeChatWithSupport
{
    /// <summary>
    /// This section is for support communication,
    /// Things like selecting user-created groups, sending messages and files by the administrator to the user
    /// </summary>

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

        // When When the administrator is connected

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(
                "ActiveRooms",
                await _chatRoomService.GetAllRooms());

            await base.OnConnectedAsync();
        }

        //When the administrator is disconnected

        [Authorize]
        public override async Task OnDisconnectedAsync(Exception exception)
        {            
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Group(roomId).SendAsync(
                "ReceiveMessage",
                "Support",
                DateTime.Now,
                "The user disconected !");
            await base.OnDisconnectedAsync(exception);
        }
        /// <summary>
        /// This method is used to set a group called a manager
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task GetRoomID(Guid roomId)
        {
            var id = Context.ConnectionId;

            var room = await _chatRoomService.GetRoom(roomId);
            await Clients.AllExcept(id).SendAsync(
                "GetRoomIdForFilterRooms",room.Id);
        }
        /// <summary>
        /// Used to send manager messages to the user
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="text"></param>
        /// <returns></returns>

        [Authorize]
        public async Task SendAgentMessage(Guid roomId, string text)
        {
            var message = new ChatMessage
            {
                SenderName = Context.User.Identity.Name,
                Text = text,
            };

            await _chatRoomService.AddMessage(roomId, message);

            await _chatHub.Clients
                .Group(roomId.ToString())
                .SendAsync("ReceiveMessage",
                    message.SenderName,
                    message.Text);

            await Clients.Caller.SendAsync("RunTimer");
                
        }

        /// <summary>
        /// Used to display messages of a group exchanged by the administrator and the user
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// To pass the group code to View and bind it in the hidden input tag to refer to the action of sending the file
        /// </summary>
        /// <returns></returns>

        [Authorize]
        public async Task GetRoomIdForUpFile()
        {
            var roomId = Context.User.FindFirstValue("RoomId");
            await Clients.Caller.SendAsync("PassRoomId", roomId);
        }
    }
}
