using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public class PagedList<T> : Convert<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }


        public PagedList() { }
        public PagedList(T data, int pageNumber, int pageSize, int totalRecords)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            this.Data = data;
            TotalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);
        }
    }
}
