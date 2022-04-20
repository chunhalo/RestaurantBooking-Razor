using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class RegisterModel
    {
        [MaxLength(256)]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[1-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not fulfill the requirements")]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string comparePassword { get; set; }
        [RegularExpression("^[0-9]{7,20}$", ErrorMessage = "Phone is not valid. It should contains only digit with mininum length of 7 and maximum length of 20")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }
    }
}
