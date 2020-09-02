using RealTimeChatWithSupport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Services
{
    public interface IChatRoomService
    {
        Task<Guid> CreateRoom(string connectionId);

        Task<ChatRoom> GetRoomForConnectionId(string connectionId);

        Task<ChatRoom> SetRoomName(Guid roomId, string name);

        Task AddMessage(Guid roomId, ChatMessage message);

        Task<List<ChatMessage>> GetMessageHistory(Guid roomId);

        Task<List<ChatRoom>> GetAllRooms();

        Task<ChatRoom> GetRoom(Guid roomId);

    }
}
