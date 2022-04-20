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
    public class UserBookingHistoryModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        public int GetPageSize { get; set; } = 10;
        [BindProperty]
        public PagedList<List<ReturnPendingBooking>> bookingList { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string PageNumberWarning { get; set; }


        public UserBookingHistoryModel(IConfiguration configuration)
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
                var searchText = user;
                var choice = "username";

                    using (var httpClient = new HttpClient())
                    {
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(searchText), "searchText");
                        content.Add(new StringContent(choice), "choice");

                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Booking/BookingWithIdPageList?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize, content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Control Page");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                string apiResponse1 = await response.Content.ReadAsStringAsync();
                                PagedList<List<ReturnPendingBooking>> getBookingjson = JsonConvert.DeserializeObject<PagedList<List<ReturnPendingBooking>>>(apiResponse1);

                                if (getBookingjson.TotalPages == 0)
                                {   
                                    bookingList = new PagedList<List<ReturnPendingBooking>>();
                                    bookingList.Data = new List<ReturnPendingBooking>();
                                }
                                else if (GetPageNumber > getBookingjson.TotalPages)
                                {
                                    PageNumberWarning = "The page number is only until " + getBookingjson.TotalPages + " pages";
                                }
                                else
                                {
                                    bookingList = getBookingjson;
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
                return RedirectToPage("/UserError");
            }
        }

    }
}

