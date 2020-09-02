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
    public class MemoryChatRoomService : IChatRoomService
    {
        private IHttpContextAccessor _httpAccessor { get; }
        private IServiceProvider _sp;
        public MemoryChatRoomService(IHttpContextAccessor httpAccessor,IServiceProvider sp)
        {
            _httpAccessor = httpAccessor;
            _sp = sp;
        }


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
