using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using NLog;
using RestaurantBooking_Razor.Model;

namespace RestaurantBooking_Razor.Pages.AnnouncePage
{
    public class PostAnnouncementModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        public PostAnnouncementModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [TempData]
        public string MessageKey{get;set;}
        [BindProperty]
        public AnnouncementAddModel announcementAddModel { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    byte[] data;
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(announcementAddModel.Title), "Title");
                    content.Add(new StringContent(announcementAddModel.Description), "Description");

                    using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Announcement", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            MessageKey = "Announcement has been created and sent to all user successfully";
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin posted an announcement in Admin Announcement Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            return RedirectToPage("/AnnouncePage/AnnouncementHome");
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //return Unauthorized()
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Announcement Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");


                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Announcement Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            return RedirectToPage("/PrivilegeError");
                        }
                    }


                }
                return Page();
               


            }
            catch (Exception e)
            {
                return Page();
            }
        }
    }
}
