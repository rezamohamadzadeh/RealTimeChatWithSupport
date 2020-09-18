using RealTimeChatWithSupport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Services
{
    public interface IChatRoom
    {
        Task<Guid> CreateRoom(string connectionId);

        Task<Models.ChatRoom> GetRoomForConnectionId(string connectionId);

        Task<Models.ChatRoom> SetRoomName(Guid roomId, string name);

        Task AddMessage(Guid roomId, ChatMessage message);

        Task<List<ChatMessage>> GetMessageHistory(Guid roomId);

        Task<List<Models.ChatRoom>> GetAllRooms();

        Task<Models.ChatRoom> GetRoom(Guid roomId);

        Task<List<Question>> GetQuestionsForm();

    }
}
