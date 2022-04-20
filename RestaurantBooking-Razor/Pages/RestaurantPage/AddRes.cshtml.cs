using System;
using System.Collections.Generic;
using System.IO;
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
    public class AddResModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        public AddResModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [BindProperty]
        public ResAddModel res { get; set; }

        [TempData]
        public string TransferMessage { get; set; }
        public async Task<IActionResult> OnGet(int id, int resId)
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(id.ToString()), "TableId");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Authentication/validate"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Add Restaurant Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            return Page();

                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            //Unauthorize
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Add Restaurant Page");
                            logger.Log(msg);
                            //MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Add Restaurant Page");
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
                var getstarttime = res.OperationStart.Split(":");
                var starttimehour = getstarttime[0];
               
                var starttimemin = getstarttime[1];
                var getendtime = res.OperationEnd.Split(":");
                var endtimehour = getendtime[0];  
                var endtimemin = getendtime[1];

                var comparestarttime = starttimehour + "." + starttimemin;
                var compareendtime = endtimehour + "." + endtimemin;
                var totalTime = Convert.ToDecimal(compareendtime) - Convert.ToDecimal(comparestarttime);


                if(totalTime < 3 && totalTime >=0)
                {
                    ViewData["GreaterError"] = "Operation Between Start Time and End time must be at least 3 hour";
                    return Page();
                }
                else if(Convert.ToDateTime(res.OperationEnd) < Convert.ToDateTime(res.OperationStart))
                {
                    ViewData["GreaterError"] = "Operation End time must be greater than Start Time";
                    return Page();
                }
                //else if (Convert.ToDecimal(comparestarttime) >= Convert.ToDecimal(compareendtime))
                //{
                //    ViewData["GreaterError"] = "Operation Start Time should not bigger than operation End Time";
                //    return Page();
                //}


                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    byte[] data;
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(res.Name), "Name");
                    content.Add(new StringContent(res.Address), "Address");
                    content.Add(new StringContent(res.Phone), "Phone");    
                    content.Add(new StringContent(res.Description), "Description");
                    content.Add(new StringContent(res.OperationStart.ToString()), "OperationStart");
                    content.Add(new StringContent(res.OperationEnd.ToString()), "OperationEnd");

                    if (res.Image != null)
                    {
                        using (var br = new BinaryReader(res.Image.OpenReadStream()))
                        {
                            data = br.ReadBytes((int)res.Image.OpenReadStream().Length);
                        }
                        ByteArrayContent bytes = new ByteArrayContent(data);
                        content.Add(bytes, "Image", res.Image.FileName);
                    }
                    using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Restaurant", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            int getResId = JsonConvert.DeserializeObject<int>(apiResponse);
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Restaurant with name," + res.Name + " has been created successfully");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            TransferMessage = "Restaurant has been created. Fill in accomodate to finish configuration";
                            return RedirectToPage("EditTable", new { id = getResId });
                        }
                        else
                        {
                            ViewData["NameExist"] = "Restaurant Name exist";
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
