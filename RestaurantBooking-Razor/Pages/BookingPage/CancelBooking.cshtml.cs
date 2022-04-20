using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using NLog;

namespace RestaurantBooking_Razor.Pages.BookingPage
{
    public class CancelBookingModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        [TempData]
        public string MessageKey { get; set; }

        public CancelBookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int id, int? GetPageNumber)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                var getrole = HttpContext.Request.Cookies["Role"];

                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(id.ToString()), "BookId");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.PutAsync(_configuration["Uri"] + "api/Booking/CancelBooking", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            if (getrole == "Admin")
                            {
                                if (GetPageNumber == null)
                                {
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin cancel booking with id," + id);
                                    msg.Properties.Add("user", user);
                                    logger.Log(msg);
                                    return RedirectToPage("PendingBooking");
                                }
                                else
                                {
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User cancel booking with id," + id);
                                    msg.Properties.Add("user", user);
                                    logger.Log(msg);
                                    return RedirectToPage("PendingBooking", new { GetPageNumber = GetPageNumber });
                                }
                            }
                            else
                            {
                                if (GetPageNumber == null)
                                {
                                    return RedirectToPage("UserBookingHistory");
                                }
                                else
                                {
                                    return RedirectToPage("UserBookinghistory", new { GetPageNumber = GetPageNumber });
                                }
                            }
                            
                            

                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Manage User Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
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
                return RedirectToPage("/UserError");
            }
        }
    }
}
