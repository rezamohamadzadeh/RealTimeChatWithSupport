using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Internal;
using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Dtos;
using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Utility;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Controllers
{
    /// <summary>
    /// Use this controller for admin users
    /// </summary>
    [Authorize(Roles ="Admin")]
    public class AgentController : BaseController
    {
        private readonly IHubContext<ChatHub> _agentHub;
        private readonly ApplicationContext _context;

        public AgentController(IHubContext<ChatHub> agentHub,ApplicationContext context)
        {
            _agentHub = agentHub;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Upload Files in chat 
        /// </summary>
        /// <param name="modeldto"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> GetFile()
        {
            try
            {
                var File = Request.Form.Files[0];
                var RoomId = Request.Form["RoomId"].FirstOrDefault();

                if(File == null || string.IsNullOrEmpty(RoomId))
                    return Json(new { error = "The parameters is invalid !" });


                var fileExtention = Path.GetExtension(File.FileName);
                var fileName = Guid.NewGuid().ToString() + fileExtention;
                string savePath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot/" + RoomId + "/", fileName
                );
                string DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + RoomId + "/");
                Upload uploader = new Upload();
                string remotePath = Request.Scheme + "://" + Request.Host.Value + "/" + RoomId + "/" + fileName;
                await uploader.UploadImage(savePath, DirectoryPath, File);

                var model = new ChatMessage
                {
                    ChatRoomId = new Guid(RoomId),
                    SenderName = User.FindFirstValue(ClaimTypes.Name),
                    Text = !CheckFileIsImage(fileExtention) ?
                    "<a class='text-white' href=" + remotePath + " target='_blank'> Download File </a>" :
                    "<img src=" + remotePath + " alt='Image' height='250' width='250'/>"
                };
                await _context.ChatMessages.AddAsync(model);
                await _context.SaveChangesAsync();
                await _agentHub.Clients
                        .Group(RoomId.ToString())
                        .SendAsync("ReceiveMessage",
                            model.SenderName,
                            model.DateTime,
                            model.Text);

                return Json(new { message = "The operation success \n"});
            }
            catch (Exception e)
            {
                return Json(new { error = "The operation failed \n" + e.Message });
            }

        }
    }
}
