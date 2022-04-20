using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NLog;

namespace RestaurantBooking_Razor.Pages.LoginPage
{
    public class LogoutModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IActionResult OnGet()
        {
            try
            {
                var user = HttpContext.Request.Cookies["User"];
                HttpContext.Response.Cookies.Delete("JWTToken");
                var tempuser = user;
                HttpContext.Response.Cookies.Delete("User");
                HttpContext.Response.Cookies.Delete("Role");
                var msg = new LogEventInfo(LogLevel.Info, logger.Name, tempuser + " has successfully logout");
                msg.Properties.Add("user", tempuser);
                logger.Log(msg);
                return RedirectToPage("Login");
            }
            catch (Exception e)
            {
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/LoginError");
            }
        }
    }
}
