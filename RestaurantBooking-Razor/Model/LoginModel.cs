using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class LoginModel
    {
        [MaxLength(256)]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
