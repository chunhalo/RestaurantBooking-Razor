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

namespace RestaurantBooking_Razor.Pages.LoginPage
{
    public class RegisterAdminModel : PageModel
    {

        private readonly IConfiguration _configuration;
        public RegisterAdminModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public Model.RegisterModel register { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        public async Task<IActionResult> OnGet()
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "/api/Authentication/validate"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access to Admin Register Admin Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            return Page();
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Register Admin Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }

                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Register Admin Page");
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

        public async Task<IActionResult> OnPost()
        {

            try
            {

                if (!ModelState.IsValid)
                {
                    return Page();
                }
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);

                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(register.Username), "Username");
                    content.Add(new StringContent(register.Email), "Email");
                    content.Add(new StringContent(register.Password), "Password");
                    content.Add(new StringContent(register.PhoneNumber), "PhoneNumber");
                    content.Add(new StringContent("https://localhost:44360/LoginPage/EmailConfirmation"), "clientlink");

                    using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Authentication/RegisterAdmin", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin has registered an admin account with username," + register.Username + " successfully without email confirmation");
                            //msg.Properties.Add("user", user);
                            logger.Log(msg);
                            MessageKey = "Admin," + register.Username + "has been registered successfully. Please contact the registered admin to confirm his email";
                            return RedirectToPage("/BookingPage/PendingBooking");
                        }
                        else
                        {
                            var getresponse = await response.Content.ReadAsStringAsync();
                            var convertresponse = JsonConvert.DeserializeObject<Response>(getresponse);
                            if (convertresponse.Status == "UserExist")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'User Exist' has prompted in Register Page");
                                //msg.Properties.Add("user", user);
                                logger.Log(msg);
                                ViewData["UserExist"] = convertresponse.Message;
                            }
                            else if (convertresponse.Status == "EmailExist")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Error message 'Email Exist' has prompted in Register Page");
                                //msg.Properties.Add("user", user);
                                logger.Log(msg);
                                ViewData["EmailExist"] = convertresponse.Message;
                            }


                            return Page();
                        }

                    }
                }
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
