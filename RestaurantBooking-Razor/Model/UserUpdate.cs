using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class UserUpdate
    {
        [MaxLength(50)]
        [Display(Name = "Old password")]
        [Required(ErrorMessage = "Old Password is required")]
        public string old_password { get; set; }
        [Display(Name = "New password")]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*[1-9])(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password does not fulfill the requirements")]
        public string password { get; set; }
        [Display(Name = "Confirm Password")]
        [Required]
        [Compare("password")]
        public string comparePassword { get; set; }
    }

    public class GetUsers
    {
        public string username { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string email { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone is not valid")]
        [MinLength(5, ErrorMessage = "Phone Number should have at least 5 digit")]
        [MaxLength(20, ErrorMessage = "Phone number should not exceed 20 digit")]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        public string phoneNumber { get; set; }
        //public IEnumerable<string> roles { get; set; }
        public string roles { get; set; }
        public string status { get; set; }
    }

    public class Status
    {
        public string statusName { get; set; }
    }

}
