//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class TravelOffer
    {
        public TravelOffer()
        {
            this.ExtendedInformation = new HashSet<ExtendedInformation>();
        }
    
        public int OfferId { get; set; }
        public double PricePerPerson { get; set; }
        public string Place { get; set; }
        public int HotelRating { get; set; }
        public string HotelName { get; set; }
        public string TravelType { get; set; }
        public int DayCount { get; set; }
        public string BoardType { get; set; }
        public int CountryId { get; set; }
        public string ImgPath { get; set; }
    
        public virtual Country Country { get; set; }
        public virtual ICollection<ExtendedInformation> ExtendedInformation { get; set; }
    }
}
