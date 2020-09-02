using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Utility;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeChatWithSupport.Controllers
{
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

        [HttpPost]
        public virtual async Task<IActionResult> Index(SendFileDto modeldto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var fileExtention = Path.GetExtension(modeldto.File.FileName);
                    var File = Guid.NewGuid().ToString() + fileExtention;
                    string savePath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/" + modeldto.RoomId + "/", File
                    );
                    string DirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + modeldto.RoomId + "/");
                    Upload uploader = new Upload();
                    string remotePath = Request.Scheme + "://" + Request.Host.Value + "/" + modeldto.RoomId + "/" + File;
                    await uploader.UploadImage(savePath, DirectoryPath, modeldto.File);

                    var model = new ChatMessage
                    {
                        ChatRoomId = new Guid(modeldto.RoomId),
                        SenderName = User.FindFirstValue(ClaimTypes.Name),
                        SentAt = DateTimeOffset.Now,
                        Text = !CheckFileIsImage(fileExtention) ?
                        "<a href=" + remotePath + " target='_blank'> Download File </a>" :
                        "<img src=" + remotePath + " alt='Image' height='250' width='250'/>"
                    };
                    await _context.ChatMessages.AddAsync(model);
                    await _context.SaveChangesAsync();
                    await _agentHub.Clients
                            .Group(modeldto.RoomId.ToString())
                            .SendAsync("ReceiveMessage",
                                model.SenderName,
                                model.SentAt,
                                model.Text);
                    
                    return View();
                }
                catch (Exception e)
                {
                    return Json(new { error = "The operation failed \n" + e.Message });
                }
            }

            return View();
        }
    }
}
