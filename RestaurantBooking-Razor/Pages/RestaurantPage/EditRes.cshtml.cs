using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using RestaurantBooking_Razor.Model;

namespace RestaurantBooking_Razor.Pages.RestaurantPage
{
    public class EditResModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty]
        public Restaurant res { get; set; }
        [BindProperty]
        public List<RestaurantStatus> ResStatus { get; set; }
        [BindProperty]
        public List<RestaurantStatus> ResAllStatus { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        [TempData]
        public string GreaterError { get; set; }

        public IFormFile GetImage { get; set; }

        private readonly IConfiguration _configuration;
        public EditResModel(IConfiguration configuration)
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
                            using (var response4 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetRestaurantStatuses"))
                            {
                                string apiResponse1 = await response4.Content.ReadAsStringAsync();
                                List<RestaurantStatus> getResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse1);
                                ResStatus = getResStatus;
                                using (var response2 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/" + id))
                                {
                                    if (response2.IsSuccessStatusCode)
                                    {
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit Restaurant Page with id, " + id);
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                        string apiResponse = await response2.Content.ReadAsStringAsync();
                                        Restaurant getRes = JsonConvert.DeserializeObject<Restaurant>(apiResponse);
                                        if (getRes == null)
                                        {
                                            res = new Restaurant();
                                        }
                                        else
                                        {
                                            res = getRes;

                                        }
                                        using (var response3 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetAllRestaurantStatuses"))
                                        {
                                            if (response3.IsSuccessStatusCode)
                                            {
                                                string apiResponse2 = await response3.Content.ReadAsStringAsync();
                                                List<RestaurantStatus> getAllResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse2);
                                                ResAllStatus = getAllResStatus;
                                            }
                                        }
                                    }
                                    else if ((int)response2.StatusCode == 404)
                                    {
                                        res = new Restaurant();
                                        var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Restaurant with id, " + id + " not found in Admin Edit Restaurant Page");
                                        msg.Properties.Add("user", user);
                                        logger.Log(msg);
                                        ViewData["NotFound"] = "Restaurant Id not found";
                                    }
                                }
                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit Restaurant Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit Restaurant Page");
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

                var getstarttime = res.OperationStart.Split(":");
                var starttimehour = getstarttime[0];

                var starttimemin = getstarttime[1];
                var getendtime = res.OperationEnd.Split(":");
                var endtimehour = getendtime[0];
                var endtimemin = getendtime[1];

                var comparestarttime = starttimehour + "." + starttimemin;
                var compareendtime = endtimehour + "." + endtimemin;
                var totalTime = Convert.ToDecimal(compareendtime) - Convert.ToDecimal(comparestarttime);


                if (totalTime < 3 && totalTime >= 0)
                {
                    ViewData["GreaterError"] = "Operation Between Start Time and End time must be at least 3 hour";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        if (res.Status == 2)
                        {

                            using (var response3 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetAllRestaurantStatuses"))
                            {
                                if (response3.IsSuccessStatusCode)
                                {
                                    string apiResponse2 = await response3.Content.ReadAsStringAsync();
                                    List<RestaurantStatus> getAllResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse2);
                                    ResAllStatus = getAllResStatus;
                                }
                            }

                        }
                        else
                        {
                            using (var response4 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetRestaurantStatuses"))
                            {
                                string apiResponse1 = await response4.Content.ReadAsStringAsync();
                                List<RestaurantStatus> getResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse1);
                                ResStatus = getResStatus;
                            }
                        }
                    }
                    return Page();
                   // return RedirectToAction("OnGet", new { id=res.ResId });
                }
                else if (Convert.ToDateTime(res.OperationEnd) < Convert.ToDateTime(res.OperationStart))
                {
                    ViewData["GreaterError"] = "Operation End time must be greater than Start Time";
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        if (res.Status == 2)
                        {
                            using (var response3 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetAllRestaurantStatuses"))
                            {
                                if (response3.IsSuccessStatusCode)
                                {
                                    string apiResponse2 = await response3.Content.ReadAsStringAsync();
                                    List<RestaurantStatus> getAllResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse2);
                                    ResAllStatus = getAllResStatus;
                                }
                            }
                        }
                        else
                        {
                            using (var response4 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/GetRestaurantStatuses"))
                            {
                                string apiResponse1 = await response4.Content.ReadAsStringAsync();
                                List<RestaurantStatus> getResStatus = JsonConvert.DeserializeObject<List<RestaurantStatus>>(apiResponse1);
                                ResStatus = getResStatus;
                            }
                        }
                    }
                   return Page();
                }
               
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    byte[] data;
                    
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(res.ResId.ToString()), "ResId");
                    content.Add(new StringContent(res.Name), "Name");
                    content.Add(new StringContent(res.Address), "Address");
                    content.Add(new StringContent(res.Phone), "Phone");
                    content.Add(new StringContent(res.Description), "Description");
                    content.Add(new StringContent(res.OperationStart), "OperationStart");
                    content.Add(new StringContent(res.OperationEnd), "OperationEnd");
                    content.Add(new StringContent(res.Status.ToString()), "Status");
                    content.Add(new StringContent(res.Image), "Image");

                    if (GetImage != null)
                    {

                        using (var br = new BinaryReader(GetImage.OpenReadStream()))
                        {
                            data = br.ReadBytes((int)GetImage.OpenReadStream().Length);
                        }
                        ByteArrayContent bytes = new ByteArrayContent(data);
                        content.Add(bytes, "ImageFile", GetImage.FileName);
                    }
                    await httpClient.PutAsync(_configuration["Uri"]+"api/Restaurant/" + res.ResId, content);

                }
                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin updated the restaurant with restaurant name, " + res.Name + " successfully");
                msg.Properties.Add("user", user);
                logger.Log(msg);
                MessageKey = "Restaurant has been edited successfully";
                return RedirectToPage("AdminRes");

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
