using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class ResetPasswordModelClass
    {
        [MaxLength(50)]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[1-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not fulfill the requirements")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class EmailToken
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
