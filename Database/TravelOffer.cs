//------------------------------------------------------------------------------
// <auto-generated>
//    Dieser Code wurde aus einer Vorlage generiert.
//
//    Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten Ihrer Anwendung.
//    Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
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
        public int CategoryId { get; set; }
        public string ImgPath { get; set; }
        public Nullable<byte> Image { get; set; }
        public bool TopOffer { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual ICollection<ExtendedInformation> ExtendedInformation { get; set; }
    }
}
