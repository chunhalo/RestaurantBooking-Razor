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

namespace RestaurantBooking_Razor.Pages.BookingPage
{
    public class SetConfirmDateModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        [TempData]
        public string MessageKey { get; set; }
        [TempData]
        public string WarningMessage { get; set; }



        public SetConfirmDateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int id,string searchText1, string choice, int? GetPageNumber)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(id.ToString()), "BookId");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.PutAsync(_configuration["Uri"] + "api/Booking/SetConfirmDate", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var getresponse = await response.Content.ReadAsStringAsync();
                            var convertresponse = JsonConvert.DeserializeObject<Response>(getresponse);
                            if (convertresponse.Status == "Modified")
                            {
                                WarningMessage = convertresponse.Message;
                            }
                            else if (convertresponse.Status == "Success")
                            {
                                MessageKey = convertresponse.Message;
                            }

                            if (GetPageNumber == null)
                            {
                                return RedirectToPage("ValidateBooking", new { searchText1 = searchText1, choice = choice });
                            }
                            else
                            {
                                return RedirectToPage("ValidateBooking", new { GetPageNumber=GetPageNumber,searchText1 = searchText1, choice = choice });
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
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Manage User Page");
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
