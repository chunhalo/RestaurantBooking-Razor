using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class Restaurant
    {
        public int ResId { get; set; }
        [Display(Name = "Restaurant Name")]
        [Required(ErrorMessage = "Restaurant Name is required")]
        
        public string Name { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [MinLength(10, ErrorMessage = "Address should have at least 10 character")]
        [MaxLength(200, ErrorMessage = "Address should be less than 200 character")]
        public string Address { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone is not valid")]
        [MinLength(5, ErrorMessage = "Phone Number should have at least 5 digit")]
        [MaxLength(20, ErrorMessage = "Phone number should not exceed 20 digit")]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Image is required")]
        public string Image { get; set; }
        public int Status { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [MinLength(2, ErrorMessage = "Description should have at least 2 character")]
        [MaxLength(200, ErrorMessage = "Description should be less than 200 character")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Operation Start Time is required")]
        [Display(Name = "Operation Start Time")]
        
        public string OperationStart { get; set; }
        [Display(Name = "Operation End Time")]
        [Required(ErrorMessage = "Operation End Time is required")]
        public string OperationEnd { get; set; }
    }

    public class ResAddModel
    {
        [Display(Name = "Restaurant Name")]
        [Required(ErrorMessage = "Restaurant Name is required")]
        [MinLength(2, ErrorMessage = "Restaurant Name should have at least 2 character")]
        [MaxLength(100, ErrorMessage = "Restaurant Name should be less than 100 character")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [MinLength(10, ErrorMessage = "Address should have at least 10 character")]
        [MaxLength(200, ErrorMessage = "Address should be less than 200 character")]
        public string Address { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone is not valid")]
        [MinLength(5, ErrorMessage = "Phone Number should have at least 5 digit")]
        [MaxLength(20, ErrorMessage = "Phone number should not exceed 20 digit")]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Image is required")]
        public IFormFile Image { get; set; }
        public int Status { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MinLength(2, ErrorMessage = "Description should have at least 2 character")]
        [MaxLength(200, ErrorMessage = "Description should be less than 200 character")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Operation Start Time is required")]
        [Display(Name = "Operation Start Time")]
        public string OperationStart { get; set; }
        [Display(Name = "Operation End Time")]
        [Required(ErrorMessage = "Operation End Time is required")]
        public string OperationEnd { get; set; }
    }

    public class ReturnResWithStatus
    {
        public int ResId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Image { get; set; }
        public RestaurantStatus restaurantStatus { get; set; }
        public string Description { get; set; }
        public string OperationStart { get; set; }
        public string OperationEnd { get; set; }
    }
}
