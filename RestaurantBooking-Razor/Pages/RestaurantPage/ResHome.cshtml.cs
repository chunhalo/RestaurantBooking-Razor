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

namespace RestaurantBooking_Razor.Pages.RestaurantPage
{
    public class ResHomeModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [TempData]
        public string MessageKey { get; set; }
        public int GetPageSize { get; set; } = 6;

        [BindProperty]
        public PagedList<List<Restaurant>> resList { get; set; }

        [TempData]
        public string PageNumberWarning { get; set; }
        private readonly IConfiguration _configuration;

        public ResHomeModel(IConfiguration configuration)
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

                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/ActiveRestaurants?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User successful edit table in User Home Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            PagedList<List<Restaurant>> getresList = JsonConvert.DeserializeObject<PagedList<List<Restaurant>>>(apiResponse);
                            if (getresList.TotalPages == 0)
                            {
                                resList = new PagedList<List<Restaurant>>();
                                resList.Data = new List<Restaurant>();
                            }
                            else if (GetPageNumber > getresList.TotalPages)
                            {
                                PageNumberWarning = "The page number is only until " + getresList.TotalPages + " pages";
                            }

                            else
                            {
                                resList = getresList;

                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //return Unauthorize()
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Home Page");
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

        public async Task<IActionResult> OnPost()
        {
            return Page();
        }
    }
}
