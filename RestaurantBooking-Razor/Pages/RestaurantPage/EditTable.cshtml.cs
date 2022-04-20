using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using RestaurantBooking_Razor.Model;

namespace RestaurantBooking_Razor.Pages.RestaurantPage
{
    public class EditTableModel : PageModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        [BindProperty(SupportsGet = true)]
        public Restaurant res { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<Table> tableList { get; set; }
        [TempData]
        public string MessageKey { get; set; }
        [TempData]
        public string AmountWarning { get; set; }
        [TempData]
        public string TransferMessage { get; set; }


        public class getJson 
        { 
            public int jsonid { get; set; }
            public int resId { get; set; }
        }

        public class ajaxTableList
        {
            public List<Table> TableList { get; set; }
            public int resId { get; set; }
        }

        private readonly IConfiguration _configuration;

        [TempData]
        public string ajaxtableList { get; set; }
        public EditTableModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> OnGet(int id)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return Page();
                //}
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Authentication/validate"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var response2 = await httpClient.GetAsync(_configuration["Uri"] + "api/Restaurant/" + id))
                            {
                                if (response2.IsSuccessStatusCode)
                                {
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin access Edit Product Page with id, " + id);
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
                                        tableList = new List<Table>();
                                        using (var response3 = await httpClient.GetAsync(_configuration["Uri"] + "api/Table/GetTableByResId?id=" + res.ResId))
                                        {
                                            if (response3.IsSuccessStatusCode)
                                            {
                                                string apiResponse2 = await response3.Content.ReadAsStringAsync();
                                                List<Table> getTable = JsonConvert.DeserializeObject<List<Table>>(apiResponse2);
                                                tableList = getTable;
                                                
                                                //if (getTable.Count > res.AmountSlot)
                                                //{

                                                //    tableList = getTable;
                                                //    var addamountslot = res.AmountSlot + 1;
                                                //    int amount = getTable.Count - res.AmountSlot;
                                                //    AmountWarning = "You have to delete " + amount + " tables from table " + addamountslot + " to table " + getTable.Count + " to finish configuration";
                                                //}
                                                //else if (res.AmountSlot > getTable.Count)
                                                //{
                                                //    tableList = getTable;

                                                //    for(int i = getTable.Count; i < res.AmountSlot; i++)
                                                //    {
                                                //        var tableNo = i + 1;
                                                //        Table newTable = new Table { TableNo=tableNo};
                                                //        tableList.Add(newTable);
                                                //    }

                                                //}

                                                //else if (res.AmountSlot == getTable.Count)
                                                //{
                                                //    AmountWarning = "";
                                                //    tableList = getTable;
                                                //}
                                                //else if (getTable.Count == 0)
                                                //{
                                                //    for (int i = 0; i < res.AmountSlot; i++)
                                                //    {
                                                //        Table newTable = new Table();
                                                //        tableList.Add(newTable);
                                                //    }
                                                //    AmountWarning = "Please configure all the amount of accomodate for each table";
                                                //}

                                            }
                                        }

                                    }


                                }
                                else if ((int)response2.StatusCode == 404)
                                {
                                    res = new Restaurant();

                                    tableList = new List<Table>();
                                    var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Product with id, " + id + " not found in Admin Edit Product Page");
                                    msg.Properties.Add("user", user);
                                    logger.Log(msg);
                                    ViewData["NotFound"] = "Restaurant Id not found";
                                }
                            }
                        }
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit Product Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit Product Page");
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
                //if (product.Stock == 0 && product.Status == 1)
                //{
                //    MessageKey = "0 quantity of stock cannot set to active";
                //    return RedirectToAction("OnGet", new { id = product.ProductId });
                //}

                //List<Table> addtableList = Request.Form["hiddenlist"];
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];
                
                List<Table> tables = new List<Table>();
                checkTable check = checkTable.Correct;
                foreach (Table table in tableList)
                {
                    if (table.Accommodate == 0)
                    {
                        check = checkTable.IsZero;
                    }
                    else if (table.Accommodate >= 100)
                    {
                        check = checkTable.OverHundred;
                    }
                    Table newTable = new Table
                    {
                        TableId = table.TableId,
                        TableNo = table.TableNo,
                        Accommodate = table.Accommodate,
                        ResId = res.ResId
                    };
                    tables.Add(newTable);
                }
                if (check == checkTable.Correct)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                        var json = JsonConvert.SerializeObject(tables);


                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var resssss = await httpClient.PutAsync(_configuration["Uri"] + "api/Table", content))
                        {
                            if (resssss.IsSuccessStatusCode)
                            {
                                var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin successful edit table in Admin Edit Table Page with tableId");
                                msg.Properties.Add("user", user);
                                logger.Log(msg);
                                MessageKey = "Successful update Table";
                                return RedirectToPage("AdminRes");
                            }
                        }

                    }
                    //var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin updated the product with product name, " + product.ProductName + " successfully");
                    //msg.Properties.Add("user", user);
                    //logger.Log(msg);
                    //MessageKey = "Product has been edited successfully";
                    return RedirectToPage("AdminRes");
                }
                else if (check == checkTable.IsZero)
                {
                    //MessageKey = "Please fill up all accomodate. Accomodate should not be 0";
                    //return RedirectToAction("OnGet", new { id = res.ResId });
                    return Page();
                }
                else
                {
                    //MessageKey = "Accomodate cannot be over 100";
                    return RedirectToAction("OnGet", new { id = res.ResId });
                }

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

        public async Task<IActionResult> OnPostAdd([FromBody] ajaxTableList getTableListAjax )
        {
            try
            {
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.GetAsync(_configuration["Uri"] + "api/Table/" + getTableListAjax.resId))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            List<Table> getTable = JsonConvert.DeserializeObject<List<Table>>(apiResponse);
                            //if (getTable.Count == 0)
                            //{
                            //    Table newTable = new Table();
                            //    tableList.Add(newTable);
                            //}
                            //else
                            //{
                                var currentTableAmount = getTableListAjax.TableList.Count + 1;
                                Table newTable = new Table();
                                
                                for (int i = 0; i < getTable.Count; i++)
                                {
                                    findTable find = findTable.NotFound;
                                    
                                    for (int j = 0; j < getTableListAjax.TableList.Count; j++)
                                    {
                                        if (getTable[i].TableNo == getTableListAjax.TableList[j].TableNo)
                                        {
                                            find = findTable.Found; //mean there is data in db but ajax haven use
                                        }
                                    }

                                    if (find == findTable.NotFound)
                                    {
                                        newTable.TableNo = getTable[i].TableNo;
                                        newTable.TableId = getTable[i].TableId;
                                        return new JsonResult(newTable);

                                    }

                                }

                                for(int k=1;k<=getTableListAjax.TableList.Count; k++){
                                    findTable find1 = findTable.NotFound;
                                    for (int l = 0; l < getTableListAjax.TableList.Count; l++)
                                    {
                                        
                                        if (getTableListAjax.TableList[l].TableNo == k)
                                        {
                                            find1 = findTable.Found;
                                        }
                                    }
                                    if (find1 == findTable.NotFound)
                                    {
                                        newTable.TableNo = k;
                                        return new JsonResult(newTable);

                                    }
                                }
                                newTable.TableNo = currentTableAmount;
                                return new JsonResult(newTable);
                            }
                        //}

                        
                        else if ((int)response.StatusCode == 401)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Unauthorized user trying to access Admin Edit Product Page");
                            logger.Log(msg);
                            MessageKey = "You are required to login to acess this page";
                            return RedirectToPage("/LoginPage/Login");
                        }
                        else if ((int)response.StatusCode == 403)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "User with no privilege trying to access Admin Edit Product Page");
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

        

        public async Task<IActionResult> OnPostDelete([FromBody]getJson getJson)
        {
            try
            {
                
                var id = getJson.jsonid;
                
                var gettoken = HttpContext.Request.Cookies["JWTToken"];
                var user = HttpContext.Request.Cookies["User"];

                using (var httpClient = new HttpClient())
                {
                    var content = new MultipartFormDataContent();
                    content.Add(new StringContent(id.ToString()), "TableId");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gettoken);
                    using (var response = await httpClient.PostAsync(_configuration["Uri"] + "api/Table/DeleteTable", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var msg = new LogEventInfo(LogLevel.Info, logger.Name, "Admin delete table in Edit Table Page");
                            msg.Properties.Add("user", user);
                            logger.Log(msg);
                            List<Table> gettableList = new List<Table>();
                            using (var response2 = await httpClient.GetAsync(_configuration["Uri"] + "api/Table/GetQtyTableByResId?id=" + getJson.resId))
                            {

                                if (response2.IsSuccessStatusCode)
                                {
                                    string apiResponse2 = await response2.Content.ReadAsStringAsync();
                                    List<Table> getTable = JsonConvert.DeserializeObject<List<Table>>(apiResponse2);
                                    if (getTable.Count != 0)
                                    {

                                        gettableList = getTable;
                                    }
                                    else
                                    {
                                        tableList = getTable;
                                    }

                                    return new JsonResult(gettableList);

                                }

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
    }

        //public JsonResult OnGetAdd()
        //{
        //    Table newTable = new Table();
        //    newTable.TableNo = tableList.Count + 1;
        //    tableList.Add(newTable);
        //    return new JsonResult(newTable);
        //}
    
}
