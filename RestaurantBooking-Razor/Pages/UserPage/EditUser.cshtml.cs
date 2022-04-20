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
    public class EditUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public EditUserModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public GetUsers getUsers { get; set; }


        [TempData]
        public string MessageKey { get; set; }


        public async Task<IActionResult> OnGet(string username)
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
                    using (var response = await httpClient.GetAsync(_configuration["Uri"]+"api/Authentication/GetUserByUsername/?username=" + username))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit User Page with username, " + username);
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse1 = await response.Content.ReadAsStringAsync();
                            GetUsers jsonUser = JsonConvert.DeserializeObject<GetUsers>(apiResponse1);
                            getUsers = jsonUser;

                        }
                        else if ((int)response.StatusCode == 404)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with username, " + username + " not found in Admin Edit User Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            ViewData["NotFound"] = "User not found";
                        }


                        else if ((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit User Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit User Page");
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
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(getUsers.username), "username");
                    content.Add(new StringContent(getUsers.email), "email");
                    content.Add(new StringContent(getUsers.phoneNumber), "phoneNumber");
                    content.Add(new StringContent(getUsers.status), "status");
                    await httpClient.PutAsync(_configuration["Uri"]+"api/Authentication/EditUser", content);

                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin edited user with username, " + getUsers.username + " at Admin Edit User Page ");
                    msg.Properties.Add("user", user);
                    logger.Log(msg);
                    MessageKey = "User," + getUsers.username + " has been updated suceessfully";
                    return RedirectToPage("AdminControl");

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
