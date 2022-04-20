using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class ForgetPasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ForgetPasswordModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Email")]
        public string getEmail { get; set; }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(getEmail), "email");
                    content.Add(new StringContent("https://localhost:44360/LoginPage/ResetPassword"), "clientlink");
                    using (var response = await httpClient.PostAsync(_configuration["Uri"]+"api/Authentication/ForgetPassword", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            Response getResponse = JsonConvert.DeserializeObject<Response>(apiResponse);
                            if (getResponse.Status == "Success")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "A reset password token is sent to email, " + getEmail);
                                logger.Log(msg);
                                return RedirectToPage("ForgetPasswordConfirmation");
                            }
                            else
                            {
                                ViewData["warning"] = "Fail";
                                return Page();
                            }
                        }
                        else if ((int)response.StatusCode == 404)
                        {
                            ViewData["warning"] = "Email does not exist";
                            return Page();
                        }
                    }

                }
                return Page();
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
