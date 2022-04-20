using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class Table
    {
        public int TableId { get; set; }
        public int ResId { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Accomodate must be between 1 to 99")]
        public int Accommodate { get; set; }
        public int TableNo { get; set; }
        public int Status { get; set; }
    }

    public class EditTableWithStatus
    {
        public int TableId { get; set; }
        public int ResId { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Accomodate must be between 1 to 99")]
        public int Accommodate { get; set; }
        public int TableNo { get; set; }
        public bool status { get; set; }
    }

}
