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

namespace RestaurantBooking_Razor.Pages.UserPage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ChangePasswordModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public UserUpdate userUpdate { get; set; }
        [BindProperty]
        public Response responsemsg { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        public IActionResult OnGet()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                //Unauthorize
                if (gettoken != null)
                {
                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access User Profile Page");
                    msg.Properties.Add("user", user);
                    logger.Log(msg);
                    return Page();
                }
                else
                {
                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Profile Page");
                    logger.Log(msg);
                    MessageKey = "You are required to login to acess this page";
                    return RedirectToPage("/LoginPage/Login");
                }
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

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                var getrole = HttpContext.Request.Cookies["Role"];

                //List<Product> productList = new List<Product>();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(userUpdate.old_password), "old_password");
                    content.Add(new StringContent(userUpdate.password), "password");
                    using (var response = await httpClient.PutAsync(_configuration["Uri"]+"api/Authentication/UserUpdate", content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        Response GetResponse = JsonConvert.DeserializeObject<Response>(apiResponse);
                        if (GetResponse.Status == "Success")
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User has successfully changed the password");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            MessageKey = GetResponse.Message;
                            if (getrole == "Admin")
                            {
                                return RedirectToPage("/BookingPage/PendingBooking");
                            }
                            else
                            {
                                return RedirectToPage("/RestaurantPage/ResHome");
                            }
                        }
                        else
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User failed to change password as old password is incorrect");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            MessageKey = GetResponse.Message;
                            return RedirectToAction("OnGet");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                var user = HttpContext.Request.Cookies["User"];
                var getrole = HttpContext.Request.Cookies["Role"];
                var msg = new LogEventInfo(LogLevel.Error, logger.Name, "Exception occur here");
                msg.Properties.Add("user", user);
                msg.Exception = e;
                logger.Log(msg);
                if (getrole == "Admin")
                {
                    return RedirectToPage("/AdminError");
                }
                else
                {
                    return RedirectToPage("/UserError");
                }
            }


        }
    }
}
