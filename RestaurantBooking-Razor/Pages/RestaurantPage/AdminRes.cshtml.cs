using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class AdminResModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int GetPageSize { get; set; } = 5;
        [BindProperty]
        public PagedList<List<ReturnResWithStatus>> resList { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string PageNumberWarning { get; set; }
        private readonly IConfiguration _configuration;
        public AdminResModel(IConfiguration configuration)
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
                    using (var response = await httpClient.GetAsync(_configuration["Uri"]+"api/Authentication/validate"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Restaurant Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            using (var response2 = await httpClient.GetAsync(_configuration["Uri"]+"api/Restaurant/GetAllRestaurants?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                            {
                                string apiResponse = await response2.Content.ReadAsStringAsync();
                                PagedList<List<ReturnResWithStatus>> getReslist = JsonConvert.DeserializeObject<PagedList<List<ReturnResWithStatus>>>(apiResponse);
                                if (getReslist.TotalPages == 0)
                                {
                                    resList = new PagedList<List<ReturnResWithStatus>>();
                                    resList.Data = new List<ReturnResWithStatus>();
                                }
                                else if (GetPageNumber > getReslist.TotalPages)
                                {
                                    PageNumberWarning = "The page number is only until " + getReslist.TotalPages + " pages";
                                }
                                else
                                {
                                    resList = getReslist;
                                }

                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //return Unauthorized()
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Restaurant Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");


                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Restaurant Page");
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
