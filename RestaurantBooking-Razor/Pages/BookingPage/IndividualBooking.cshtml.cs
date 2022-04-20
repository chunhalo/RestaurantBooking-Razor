using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestaurantBooking_Razor.Model;
using NLog;

namespace RestaurantBooking_Razor.Pages.BookingPage
{
    public class IndividualBookingModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        [BindProperty]
        public Restaurant res { get; set; }
        [TempData]
        public string MessageKey { get; set; }

        [TempData]
        public string CheckExist { get; set; }
        
        [BindProperty]
        public string getOperationStart { get; set; }
        [BindProperty]
        public string getOperationEnd { get; set; }


    public IndividualBookingModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {

                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/" + id))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Individual Restaurant Booking Page with id, " + id);
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            Restaurant jsonRes = JsonConvert.DeserializeObject<Restaurant>(apiResponse);

                            res = jsonRes;
                            var OperationStartdatetime = DateTime.Parse(res.OperationStart);
                            var OperationEnddatetime = DateTime.Parse(res.OperationEnd);
                            getOperationStart = String.Format("{0:hh:mm:tt}", OperationStartdatetime);
                            getOperationEnd = String.Format("{0:hh:mm:tt}", OperationEnddatetime);

                        }




                        
                        else if ((int)response.StatusCode == 404)
                        {
                            res = new Restaurant();
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Restaurant with id, " + id + " not found in Individual Restaurant Booking Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            ViewData["NotFound"] = "Restaurant Id not found";
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access User Individual Restaurant Booking Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }

                    }

                    //using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Table/" + res.ResId))
                    //{
                    //    if (response.IsSuccessStatusCode)
                    //    {
                    //        //var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User access Individual Product Page with id, " + id);
                    //        //msg.Properties.Add("user", user);
                    //        //logger.Log(msg);
                    //        string apiResponse = await response.Content.ReadAsStringAsync();
                    //        List<Table> jsontablelist = JsonConvert.DeserializeObject<List<Table>>(apiResponse);

                    //        tableList = jsontablelist;
                    //    }
                    //}
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
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                    
                using (var httpClient = new HttpClient())
                {
                    //get datetime and table id
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    var content = new MultipartFormDataContent();

                    var getdate = Request.Form["date"];
                    var getstarttime = Request.Form["starttime"];
                    var getendtime = Request.Form["endtime"];
                    var starttimewithdate = getdate + ' ' + getstarttime;
                    var endtimewithdate = getdate + ' ' + getendtime;
                    var request = Request.Form["request"];
                    var tableId = Request.Form["flexRadioDefault"];

                    content.Add(new StringContent(res.ResId.ToString()), "ResId");
                    content.Add(new StringContent(starttimewithdate), "StartDate");
                    content.Add(new StringContent(endtimewithdate), "EndDate");
                    
                    content.Add(new StringContent(request), "request");
                    content.Add(new StringContent(tableId), "tableId");
                   

                    using (var response = await httpClient.PostAsync(_configuration["Uri"]+"api/Booking", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            Response getresponse = JsonConvert.DeserializeObject<Response>(apiResponse);

                            if (getresponse.Status == "Success")
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Booking on, " + res.Name + " from " + starttimewithdate + " to " + endtimewithdate + " with tableId, " + tableId + "  has been booked successfully");
                                logger.Log(msg);

                                MessageKey = "Your booking is successfully placed. Please wait for admin to approve";
                                return RedirectToPage("/RestaurantPage/ResHome");
                            }
                            else if(getresponse.Status=="BookedByOther")
                            {
                                CheckExist = "The table already been taken in that time";
                                return RedirectToPage("/BookingPage/IndividualBooking", new { id=res.ResId});
                            }
                            else if (getresponse.Status == "AlreadyBooked")
                            {
                                CheckExist = "You have already booked a table for this date. Please select other date for booking";
                                return RedirectToPage("/BookingPage/IndividualBooking", new { id = res.ResId });
                            }
                        }

                        return Page();
                        
                       
                    }
                }
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
