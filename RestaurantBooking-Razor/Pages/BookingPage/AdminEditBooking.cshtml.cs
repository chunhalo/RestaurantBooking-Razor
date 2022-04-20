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
    public class AdminEditBookingModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public ReturnBookingWithIntStatus booking { get; set; }
        [BindProperty]
        public List<BookingStatus> bookingStatuses { get; set; }

        [TempData]
        public string MessageKey { get; set; }

        private readonly IConfiguration _configuration;
        public AdminEditBookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<IActionResult> OnGet(int id)
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
                    using (var response = await httpClient.GetAsync(_configuration["Uri"]+"api/Authentication/validate"))
                    {
                        
                        if (response.IsSuccessStatusCode)
                        {
                            using (var response3 = await httpClient.GetAsync(_configuration["Uri"] + "api/booking/GetBookingStatus"))
                            {
                                string apiResponse1 = await response3.Content.ReadAsStringAsync();
                                List<BookingStatus> getBookingStatuses = JsonConvert.DeserializeObject<List<BookingStatus>>(apiResponse1);
                                bookingStatuses = getBookingStatuses;
                                using (var response2 = await httpClient.GetAsync(_configuration["Uri"] + "api/booking/" + id))
                                {
                                    if (response2.IsSuccessStatusCode)
                                    {
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit Booking Page with id, " + id);
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                        string apiResponse = await response2.Content.ReadAsStringAsync();
                                        ReturnBookingWithIntStatus getBooking = JsonConvert.DeserializeObject<ReturnBookingWithIntStatus>(apiResponse);
                                        if (getBooking == null)
                                        {
                                            booking = new ReturnBookingWithIntStatus();
                                        }
                                        else
                                        {
                                            booking = getBooking;
                                        }
                                    }
                                    else if ((int)response2.StatusCode == 404)
                                    {
                                        booking = new ReturnBookingWithIntStatus
                                        {
                                            res = new Restaurant()
                                        };
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Booking with id, " + id + " not found in Admin Edit Booking Page");
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                        ViewData["NotFound"] = "Booking Id not found";
                                    }
                                }
                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit Booking Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit Booking Page");
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

        public async Task<IActionResult> OnPostRefreshBookingAsync()
        {
            try {
                return RedirectToAction("OnGet", new { id = booking.bookingId });

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

        public async Task<IActionResult> OnGetUpdate()
        {
            MessageKey = "Successfully updated booking";
            booking = new ReturnBookingWithIntStatus
            {
                res = new Restaurant()
            };
            return Page();

            
        }
    }
}

