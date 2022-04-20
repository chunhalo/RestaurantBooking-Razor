using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantBooking_Razor.Model
{
    public enum checkTable
    {
        IsZero,
        OverHundred,
        Correct
    }

    public enum findTable
    {
       Found,
       NotFound
    }
}
