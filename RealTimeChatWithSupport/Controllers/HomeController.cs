using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeChatWithSupport.AppContext;
using RealTimeChatWithSupport.Dtos;
using RealTimeChatWithSupport.Models;
using RealTimeChatWithSupport.Utility;

namespace RealTimeChatWithSupport.Controllers
{

    /// <summary>
    /// Use this controller for users
    /// </summary>
    [Authorize(Roles = "Affiliate,Buyer")]
    public class HomeController : BaseController
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ApplicationContext _context;

        public HomeController(IHubContext<ChatHub> chatHub, ApplicationContext context)
        {
            _chatHub = chatHub;
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
                        Text = !CheckFileIsImage(fileExtention) ?
                        "<a href=" + remotePath + " target='_blank'> Download File </a>" :
                        "<img src=" + remotePath + " alt='Image' height='250' width='250'/>"
                    };
                    await _context.ChatMessages.AddAsync(model);
                    await _context.SaveChangesAsync();

                    await _chatHub.Clients.Group(modeldto.RoomId).SendAsync(
                    "ReceiveMessage",
                    model.SenderName,
                    model.DateTime,
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

        public async Task<JsonResult> SetVote(string id, int answer,string formid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) && answer <= 0)
                    return Json(new { success = false, message = "BadRequest !" });

                Guid newId = new Guid(id);
                Guid frmId = new Guid(formid);
                var userAnswer = await _context.QuestionAnswers.FirstOrDefaultAsync(d => d.QuestionId == newId && d.FormId == frmId);
                if (userAnswer != null)
                {
                    userAnswer.UserAnswer = (AnswerOptions)answer;
                    _context.QuestionAnswers.Update(userAnswer);

                }
                else
                {
                    var model = new QuestionAnswer
                    {
                        Question = null,
                        QuestionId = newId,
                        UserAnswer = (AnswerOptions)answer,
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        FormId = frmId
                    };
                    await _context.AddAsync(model);
                }
                
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Is Success" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error in submit voute ! \n" + ex.Message });

            }

        }
    }
}
