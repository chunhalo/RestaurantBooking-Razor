using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using RestaurantBooking_Razor.Model;

namespace RestaurantBooking_Razor.Pages.AnnouncePage
{
    public class AnnouncementHomeModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int GetPageSize { get; set; } = 5;

        [BindProperty]
        public PagedList<List<ReturnAnnouncement>> AnnouncementList { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string PageNumberWarning { get; set; }
        private readonly IConfiguration _configuration;
        public AnnouncementHomeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int? GetPageNumber)
        {
            try
            {
                if (GetPageNumber == null)
                {
                    GetPageNumber = 1;
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Announcement/AllAnnouncement?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Announcement Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            PagedList<List<ReturnAnnouncement>> getAnnouncementList = JsonConvert.DeserializeObject<PagedList<List<ReturnAnnouncement>>>(apiResponse);
                            if (getAnnouncementList.TotalPages == 0)
                            {
                                AnnouncementList = new PagedList<List<ReturnAnnouncement>>();
                                AnnouncementList.Data = new List<ReturnAnnouncement>();
                            }
                            else if (GetPageNumber > getAnnouncementList.TotalPages)
                            {
                                PageNumberWarning = "The page number is only until " + getAnnouncementList.TotalPages + " pages";
                            }
                            else
                            {
                                AnnouncementList = getAnnouncementList;
                            }


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
                var user = HttpContext.Request.Cookies["User"];
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Properties.Add("user", user);
                msg.Exception = e;
                logger.Log(msg);
                return RedirectToPage("/AdminError");
            }
        }
    }
}
