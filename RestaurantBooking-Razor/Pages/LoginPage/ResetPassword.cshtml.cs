using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using NLog;
using RestaurantBooking_Razor.Model;

namespace RestaurantBooking_Razor.Pages.LoginPage
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ResetPasswordModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public ResetPasswordModelClass resetPasswordModel { get; set; }

        [TempData]
        public string alertMessage { get; set; }

        public IActionResult OnGet(EmailToken emailToken)
        {
            try
            {
                if (emailToken != null)
                {
                    ResetPasswordModelClass resetPasswordModel1 = new ResetPasswordModelClass();

                    resetPasswordModel1.Token = emailToken.Token;
                    resetPasswordModel1.Email = emailToken.Email;
                    resetPasswordModel = resetPasswordModel1;
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

                    content.Add(new StringContent(resetPasswordModel.Password), "Password");
                    content.Add(new StringContent(resetPasswordModel.Token), "Token");
                    content.Add(new StringContent(resetPasswordModel.Email), "Email");

                    using (var response = await httpClient.PostAsync(_configuration["Uri"]+"api/Authentication/ResetPassword", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with email, " + resetPasswordModel.Email + " has reset password successfully");
                            logger.Log(msg);
                            return RedirectToPage("/LoginPage/ResetSuccess");
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
