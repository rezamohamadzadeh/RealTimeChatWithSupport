using Microsoft.AspNetCore.Mvc;

namespace RealTimeChatWithSupport.Controllers
{
    public class BaseController : Controller
    {
        protected bool CheckFileIsImage(string extention)
        {
            bool fileIsImg = false;
            switch (extention.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                    fileIsImg = true;
                    break;
                default:
                    fileIsImg = false;
                    break;
            }
            return fileIsImg;
        }
    }
}
