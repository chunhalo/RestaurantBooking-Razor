using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RestaurantBooking_Razor.Model;
using NLog;
using Newtonsoft.Json;

namespace RestaurantBooking_Razor.Pages.LoginPage
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Model.LoginModel model { get; set; }

        [BindProperty]
        public Response response { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        [TempData]
        public string alertMessage { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IConfiguration _configuration;
        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();

                    content.Add(new StringContent(model.Username), "Username");
                    content.Add(new StringContent(model.Password), "Password");


                    using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Authentication/Login", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {

                            var jsToken = await response.Content.ReadAsStringAsync();
                            var jwttoken = JsonConvert.DeserializeObject<Model.JsonToken>(jsToken);

                            HttpContext.Response.Cookies.Append("JWTToken", jwttoken.token, new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                            HttpContext.Response.Cookies.Append("User", model.Username, new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                            if (jwttoken.role.Contains("Admin"))
                            {
                                HttpContext.Response.Cookies.Append("Role", "Admin", new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin has successfully login");
                                msg.Properties.Add("user", model.Username);
                                logger.Log(msg);
                                return Redirect("BookingPage/PendingBooking");
                            }
                            else
                            {
                                HttpContext.Response.Cookies.Append("Role", "User", new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = true });
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User has successfully login");
                                msg.Properties.Add("user", model.Username);
                                logger.Log(msg);
                                return Redirect("RestaurantPage/ResHome");
                            }
                        }
                        else
                        {

                            var getresponse = await response.Content.ReadAsStringAsync();
                            var convertresponse = JsonConvert.DeserializeObject<Response>(getresponse);
                            if (convertresponse.Status == "Locked")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Account locked' has prompted in Login Page");
                                logger.Log(msg);
                            }
                            else if (convertresponse.Status == "UnconfirmedEmail")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Unconfirmed email' has prompted in Login Page");
                                logger.Log(msg);
                            }
                            else
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Invalid password' has prompted in Login Page");
                                logger.Log(msg);

                            }
                            MessageKey = convertresponse.Message;
                            return RedirectToAction("OnGet");
                        }
                    }
                }
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
