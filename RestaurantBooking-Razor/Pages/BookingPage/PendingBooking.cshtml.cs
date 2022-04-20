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

namespace RestaurantBooking_Razor.Pages.BookingPage
{
    public class PendingBookingModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        [BindProperty]
        [RegularExpression("^[0-9]{0,20}$", ErrorMessage = "Search is not valid. It should contains only digit for bookingId with mininum length of 1 and maximum length of 20")]
        [MaxLength(20)]
        public string searchtext { get; set; }

        public int GetPageSize { get; set; } = 10;
        [BindProperty]
        public PagedList<List<ReturnPendingBooking>> bookingPendingList { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string PageNumberWarning { get; set; }
        [TempData]
        public string WarningMessage { get; set; }


        public PendingBookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int? GetPageNumber,string searchText1)
        {
            try
            {
                if (GetPageNumber == null)
                {
                    GetPageNumber = 1;
                }
                
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                if (searchText1 == null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Booking/PendingBooking?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Pending Booking Page");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                string apiResponse1 = await response.Content.ReadAsStringAsync();
                                PagedList<List<ReturnPendingBooking>> getPendingBookingjson = JsonConvert.DeserializeObject<PagedList<List<ReturnPendingBooking>>>(apiResponse1);

                                if (getPendingBookingjson.TotalPages==0)
                                {
                                    bookingPendingList = new PagedList<List<ReturnPendingBooking>>();
                                    bookingPendingList.Data = new List<ReturnPendingBooking>();
                                }
                                else if (GetPageNumber > getPendingBookingjson.TotalPages)
                                {
                                    PageNumberWarning = "The page number is only until " + getPendingBookingjson.TotalPages + " pages";
                                }
                                else
                                {
                                    bookingPendingList = getPendingBookingjson;

                                }

                            }
                            else if ((int)response.StatusCode == 401)
                            {
                                //Unauthorize
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Pending Booking Page");
                                logger.Log(msg);
                                MessageKey = "You are required to login to acess this page";
                                return RedirectToPage("/LoginPage/Login");
                            }
                            else if ((int)response.StatusCode == 403)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Pending Booking Page");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                return RedirectToPage("/PrivilegeError");
                            }
                        }
                    }
                }
                else
                {
                    using (var httpClient = new HttpClient())
                    {
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(searchText1), "searchText");
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Booking/SearchPending?PageNumber=" + GetPageNumber + "&PageSize=" + GetPageSize,content))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Admin Pending Booking Page");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                string apiResponse1 = await response.Content.ReadAsStringAsync();
                                PagedList<List<ReturnPendingBooking>> getPendingBookingjson = JsonConvert.DeserializeObject<PagedList<List<ReturnPendingBooking>>>(apiResponse1);
                                if (getPendingBookingjson.TotalPages==0)
                                {
                                    bookingPendingList = new PagedList<List<ReturnPendingBooking>>();
                                    bookingPendingList.Data = new List<ReturnPendingBooking>();
                                }
                                else if (GetPageNumber > getPendingBookingjson.TotalPages)
                                {
                                    PageNumberWarning = "The page number is only until " + getPendingBookingjson.TotalPages + " pages";
                                }

                                else
                                {
                                    bookingPendingList = getPendingBookingjson;

                                }

                            }
                            else if ((int)response.StatusCode == 401)
                            {
                                //Unauthorize
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Pending Booking Page");
                                logger.Log(msg);
                                MessageKey = "You are required to login to acess this page";
                                return RedirectToPage("/LoginPage/Login");
                            }
                            else if ((int)response.StatusCode == 403)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Pending Booking Page");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                return RedirectToPage("/PrivilegeError");
                            }
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
            return RedirectToAction("OnGet", new { searchText1=searchtext});
        }
    }
}
