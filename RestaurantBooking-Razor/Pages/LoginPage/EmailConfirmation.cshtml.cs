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
    public class EmailConfirmationModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public EmailConfirmationModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public async Task<IActionResult> OnGet(EmailToken emailToken)
        {
            try
            {
                if (emailToken != null)
                {
                    using (var httpClient = new HttpClient())
                    {

                        var content = new MultipartFormDataContent();


                        content.Add(new StringContent(emailToken.Token), "Token");
                        content.Add(new StringContent(emailToken.Email), "Email");

                        using (var response = await httpClient.PostAsync(_configuration["Uri"]+"api/Authentication/EmailConfirmation", content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Email, " + emailToken.Email + " has been verified");
                                logger.Log(msg);
                                return RedirectToPage("/LoginPage/EmailSuccess");
                            }
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
