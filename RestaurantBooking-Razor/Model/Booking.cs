using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class Booking
    {
        public int BookId { get; set; }
        public int ResId { get; set; }
        public string UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public int Status { get; set; }
        public DateTime EndDate { get; set; }
        public int TableId { get; set; }
        public string Request { get; set; }
    }

    //public class AddBookingModel
    //{
    //    public int ResId { get; set; }
    //    public string StartDate { get; set; }
    //    public int PeopleAmount { get; set; }
    //    public string EndDate { get; set; }
    //    public int TableId { get; set; }
    //    public string Request { get; set; }
    //}
    public class AddBookingModel
    {
        public int ResId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int TableId { get; set; }
        public string Request { get; set; }
    }

    public class ReturnPendingBooking
    {
        public int bookingId { get; set; }
        public string resName { get; set; }
        public string username { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string status { get; set; }
        public int tableNo { get; set; }
        public string request { get; set; }

    }

    public class ReturnBookingWithIntStatus
    {
        [Display(Name = "Booking Id")]
        public int bookingId { get; set; }
        public Restaurant res { get; set; }
        public string username { get; set; }
        [Display(Name = "Start Time")]
        public DateTime startDate { get; set; }
        [Display(Name = "End Time")]
        public DateTime endDate { get; set; }
        public int status { get; set; }
        public Table table { get; set; }
        [MaxLength(200, ErrorMessage = "Request should not exceed 200 character")]
        public string request { get; set; }
    }

    public class CheckExistBooking
    {
        public bool checkExist { get; set; }
    }
}
