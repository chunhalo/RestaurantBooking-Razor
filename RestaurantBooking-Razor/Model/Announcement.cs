using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class Announcement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }


    public class AnnouncementAddModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MinLength(2, ErrorMessage = "Title should have at least 2 character")]
        [MaxLength(100, ErrorMessage = "Title should not exceed 100 character")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MinLength(2, ErrorMessage = "Description should have at least 2 character")]
        [MaxLength(500, ErrorMessage = "Desrption should not exceed 500 character")]
        public string Description { get; set; }
        
    }

    public class ReturnAnnouncement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }

    }
}
