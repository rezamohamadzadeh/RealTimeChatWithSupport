using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Services
{
    /// <summary>
    /// These functions are used to record information in the database.
    /// </summary>
    public class MemoryChatRoomService : IChatRoomService
    {
        private IHttpContextAccessor _httpAccessor { get; }
        private IServiceProvider _sp;
        public MemoryChatRoomService(IHttpContextAccessor httpAccessor,IServiceProvider sp)
        {
            _httpAccessor = httpAccessor;
            _sp = sp;
        }

        /// <summary>
        /// Store Messages in db
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task AddMessage(Guid roomId, ChatMessage message)
        {
            try
            {
                message.ChatRoomId = roomId;
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    dbContext.ChatMessages.Add(message);
                    dbContext.SaveChanges();
                }

                return Task.CompletedTask;

            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// When a new group is created, its details are stored in the database
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Task<Guid> CreateRoom(string connectionId)
        {
            try
            {
                var room = new ChatRoom
                {
                    OwnerConnectionId = connectionId,
                };
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    dbContext.ChatRooms.Add(room);
                    dbContext.SaveChanges();
                }
                return Task.FromResult(room.Id);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Get all rooms(groups) for Admin 
        /// </summary>
        /// <returns></returns>
        public Task<List<ChatRoom>> GetAllRooms()
        {
            try
            {
                List<ChatRoom> rooms;
                var userId = _httpAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    rooms = dbContext.ChatRooms.Where(d => d.UserId == userId || d.UserId == null).ToList();
                }
                return Task.FromResult(rooms);
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// Get group messages by roomId
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public Task<List<ChatMessage>> GetMessageHistory(Guid roomId)
        {
            try
            {
                List<ChatMessage> chatMessages = new List<ChatMessage>();
                var userId = _httpAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var chatRoom = dbContext.ChatRooms.FirstOrDefault(d => d.Id == roomId);
                    if (chatRoom.UserId != userId)
                        return Task.FromResult(chatMessages);

                    chatMessages = dbContext.ChatMessages.Where(d => d.ChatRoomId == roomId).OrderBy(x => x.SentAt).ToList();

                }

                return Task.FromResult(chatMessages);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Get Room values by connectionId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Task<ChatRoom> GetRoomForConnectionId(string connectionId)
        {
            try
            {
                ChatRoom foundRoom;
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    foundRoom = dbContext
                                    .ChatRooms
                                    .FirstOrDefault(x => x.OwnerConnectionId == connectionId);

                    if (foundRoom.Id == Guid.Empty)
                        throw new ArgumentException("Invalid Room ID");

                }

                return Task.FromResult(foundRoom);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// After created room, this function set room name in same room record in db
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<ChatRoom> SetRoomName(Guid roomId, string name)
        {
            try
            {
                ChatRoom room;
                using (var scope = _sp.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    room = dbContext.ChatRooms.FirstOrDefault(m => m.Id == roomId);
                    if (room == null)
                        throw new ArgumentException("Invalid Room ID");

                    room.Name = name;
                    dbContext.SaveChanges();

                }

                return Task.FromResult(room);
            }
            catch (Exception)
            {
                throw;
            }

        }


        /// <summary>
        /// Get room by roomId
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public Task<ChatRoom> GetRoom(Guid roomId)
        {
            ChatRoom room;
            using (var scope = _sp.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                room = dbContext.ChatRooms.FirstOrDefault(d => d.Id == roomId);

            }
            return Task.FromResult(room);
        }
        

    }
}
