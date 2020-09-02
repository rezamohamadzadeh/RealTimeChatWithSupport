using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealTimeChatWithSupport.Pages
{
    [Authorize(Roles = "Admin")]
    public class SupportAgentModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}