
namespace RealTimeChatWithSupport.Utility
{
    public class Delete
    {
        public void DeleteImage(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
