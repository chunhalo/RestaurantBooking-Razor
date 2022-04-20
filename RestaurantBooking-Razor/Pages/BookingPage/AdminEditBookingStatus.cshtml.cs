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
    public class AdminEditBookingStatusModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public ReturnBookingWithIntStatus booking { get; set; }
        [BindProperty]
        public List<BookingStatus> bookingStatuses { get; set; }

        [TempData]
        public string MessageKey { get; set; }

        private readonly IConfiguration _configuration;
        public AdminEditBookingStatusModel(IConfiguration configuration)
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
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Authentication/validate"))
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
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit Booking Status Page with id, " + id);
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

                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Booking with id, " + id + " not found in Admin Edit Booking Status Page");
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                        ViewData["NotFound"] = "Booking Id not found";
                                    }
                                }
                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Booking Status Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Booking Status Page");
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
                    content.Add(new StringContent(booking.bookingId.ToString()), "bookingId");
                    if (booking.request == null)
                    {
                        booking.request = "";
                    }
                    content.Add(new StringContent(booking.request), "request");
                    content.Add(new StringContent(booking.status.ToString()), "statusId");
                    await httpClient.PutAsync(_configuration["Uri"]+"api/booking/UpdateRequestStatus", content);

                }
                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin updated the status and request with booking Id, " + booking.bookingId + " successfully");
                msg.Properties.Add("user", user);
                logger.Log(msg);
                MessageKey = "Booking Status and Request has been edited successfully";
                return RedirectToPage("AdminBooking");

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
