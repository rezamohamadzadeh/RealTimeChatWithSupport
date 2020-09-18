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
    //[Authorize(Roles = "Affiliate,Buyer")]
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
        public virtual async Task<IActionResult> GetFile(string senderName)
        {
            try
            {
                var File = Request.Form.Files[0];
                var RoomId = Request.Form["RoomId"].FirstOrDefault();

                if (File == null || string.IsNullOrEmpty(RoomId) || RoomId == "undefined")
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
                    SenderName = senderName,
                    Text = !CheckFileIsImage(fileExtention) ?
                    "<a href=" + remotePath + " target='_blank' class='text-dark'> Download File </a>" :
                    "<img src=" + remotePath + " alt='Image' height='250' width='250'/>"
                };
                await _context.ChatMessages.AddAsync(model);
                await _context.SaveChangesAsync();

                await _chatHub.Clients.Group(RoomId).SendAsync(
                "ReceiveMessage",
                model.SenderName,
                model.DateTime,
                model.Text);

                return Json(new { message = "The operation success \n" });
            }
            catch (Exception e)
            {
                return Json(new { error = "The operation failed \n" + e.Message });
            }
        }

        public async Task<JsonResult> SetVote(string id, int answer,string formid,string userId)
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
                        UserId = userId,
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
