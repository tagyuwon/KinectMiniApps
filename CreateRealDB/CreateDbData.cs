﻿using Database;
using Database.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateRealDB
{
    class CreateDbData
    {

        private void CreateCountryEntries(Model1Container context)
        {

            foreach (var category in Enum.GetValues(typeof(CategoryEnum)).Cast<CategoryEnum>())
            {
                string categoryName = Enum.GetName(typeof(CategoryEnum),category);
                switch (category)
                {
                    case CategoryEnum.Beach:
                        categoryName = "Strand";
                        break;
                    case CategoryEnum.Ski:
                        categoryName = "Ski";
                        break;
                    case CategoryEnum.City:
                        categoryName = "Stadt";
                        break;
                    case CategoryEnum.Wander:
                        categoryName = "Wandern";
                        break;
                }

                var c = new Category() { CategoryName = categoryName };
                    context.CategorySet.Add(c);
            }
            SaveToDb(context);
        }

        /// <summary>
        /// Saves current context to database
        /// </summary>
        /// <param name="context">db context</param>
        private void SaveToDb(Model1Container context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Create Categories: Error in entry.");
            }
        }

        /// <summary>
        /// Creates a new offer
        /// </summary>
        /// <param name="price"></param>
        /// <param name="place"></param>
        /// <param name="rating"></param>
        /// <param name="hotelname"></param>
        /// <param name="anfahrt"></param>
        /// <param name="daycount"></param>
        /// <param name="futtertyp"></param>
        /// <param name="category"></param>
        /// <param name="image"></param>
        /// <param name="top"></param>
        /// <param name="zusatzinf"></param>
        /// <param name="zusatz2"></param>
        /// <param name="zusatz3"></param>
        public void CreateOffers(int price, String place, int rating, String hotelname, String anfahrt, int daycount, String futtertyp, CategoryEnum category, String image, bool top, String zusatzinf, String zusatz2, String zusatz3)
        {
            try
            {
                var dao = new TravelOfferDao();
                var offer = new TravelOffer()
                {
                    PricePerPerson = price,
                    Place = place,
                    ImgPath = image,
                    HotelRating = rating,
                    HotelName = hotelname,
                    TravelType = anfahrt,
                    DayCount = daycount,
                    BoardType = futtertyp,
                    CategoryId = (int)category,
                    TopOffer = top,
                };
                ExtendedInformation ei = new ExtendedInformation()
                {
                    Information = zusatzinf
                };
                offer.ExtendedInformation.Add(ei);
                if (zusatz2 != "")
                {
                    ExtendedInformation ei2 = new ExtendedInformation()
                    {
                        Information = zusatz2
                    };
                    offer.ExtendedInformation.Add(ei2);
                }
                if (zusatz3 != "")
                {
                    ExtendedInformation ei3 = new ExtendedInformation()
                    {
                        Information = zusatz3
                    };
                    offer.ExtendedInformation.Add(ei3);
                }
                offer.ExtendedInformation.Add(ei);
                dao.Save(offer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Create Offers: Error in entry.");
            }
        }

        public void CreateMyOffers()
        {
            //City
            CreateOffers(169, "Amsterdam", 4, "Radisson Red", "Busreise", 3, "Frühstücksbüffet", CategoryEnum.City, "images/City/amsterdam.jpg", false, "Busabfahrt in Birkenfeld", "", "");
            CreateOffers(290, "Berlin", 4, "Atlona", "Flugreise", 4, "Vollpension", CategoryEnum.City, "images/City/berlin.jpg", false, "Flug ab Saarbrücken", "außer Montags", "");
            CreateOffers(182, "Hamburg", 3, "Hotel hinterm Hafen", "Busreise", 3, "Halbpension", CategoryEnum.City, "images/City/hamburg.jpg", false, "Vor dem Hafen rechts", "", "");
            CreateOffers(205, "Paris", 3, "Best Ostern", "Zugreise", 2, "Halbpension", CategoryEnum.City, "images/City/paris.jpg", true, "Seine-Rundfahrt möglich", "", "");
            CreateOffers(310, "Wien", 5, "Hotel Mozarto", "Flugreise", 4, "All Inclusive", CategoryEnum.City, "images/City/wien.jpg", false, "inkl. Besuch des Mozart Museum", "", "");
            CreateOffers(240, "London", 3, "Hotel Big Bang", "Flugreise", 3, "Halbpension", CategoryEnum.City, "images/City/london.jpg", false, "5 min Fußweg zum Big Ben", "", "");
            CreateOffers(699, "Las Vegas", 5, "Cesarius Palast", "Flugreise", 5, "Vollpension", CategoryEnum.City, "images/City/lasvegas.jpg", false, "Abflug Frankfurt a.M.", "", "");
            CreateOffers(549, "New York", 3, "Hotel Staars Residenz", "Flugreise", 6, "Halbpension", CategoryEnum.City, "images/City/newyork.jpg", false, "inkl. Stadtführung", "", "");
            CreateOffers(659, "San Francisco", 4, "Hotel Blondie", "Flugreise", 7, "Halbpension", CategoryEnum.City, "images/City/sanfrancisco.jpg", false, "inkl. Cityticket", "", "");
            CreateOffers(899, "Sydney", 5, "Hotel Kängurus", "Flugreise", 14, "Halbpension", CategoryEnum.City, "images/City/sydney.jpg", true, "Besuch der Australian Open möglich", "Reise zur Kängurufarm", "");
            //todo Lizenzfreies Bild von Berlin einfügen
            //CreateOffers(170, "Berlin", 3, "one80", "Flugreise", 4, "", CategoryEnum.City, "", true, "Die Abschlussfahrt für Studenten", "vom 14.06.2013 bis 17.06.2013", "Gute Flugzeiten");
            //Beach
            CreateOffers(150, "Blankenberge", 3, "Hotel Belgie", "Eigene Anreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/blankenberge.jpg", false, "Wunderschöne klassische Einrichtung", "", "");
            CreateOffers(219, "Dominikanische Republik", 3, "Apartment de Sol", "Flugreise", 6, "Selbstverpflegung", CategoryEnum.Beach, "images/Beach/domrep.jpg", false, "", "", "");
            CreateOffers(329, "Fuerteventura", 4, "Hotel Ventura", "Flugreise", 5, "Vollpension", CategoryEnum.Beach, "images/Beach/fuerteventura.jpg", false, "Freies WLAN", "", "");
            CreateOffers(399, "Gran Canaria", 3, "Avenida de Canar", "Schiffsfahrt", 5, "All Inclusive", CategoryEnum.Beach, "images/Beach/grancanaria.jpg", false, "Tauchkurse zubuchbar", "", "");
            CreateOffers(199, "Hawaii", 3, "Magma Palace", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/haweii.jpg", true, "2 min zum Strand", "", "");
            CreateOffers(349, "Ibiza", 4, "Hotel Itzibitzi", "Flugreise", 4, "All Inclusive", CategoryEnum.Beach, "images/Beach/ibiza.jpg", true, "Pay-TV", "5 min zur Partymeile", "");
            CreateOffers(199, "Mallorca", 3, "Motel zum Bierkönig", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/mallorca.jpg", false, "Günstige Einkaufsmöglichkeiten", "Flughafen-Shuttle", "");
            CreateOffers(289, "Mauritius", 3, "Hotel Maurer", "Flugreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/mauritius.jpg", true, "300 m Stadtmitte", "Familienfreundlich", "");
            CreateOffers(99, "Nordsee", 3, "Zum trockenen Watt", "eigene Anreise", 5, "Halbpension", CategoryEnum.Beach, "images/Beach/nordsee.jpg", false, "22 h Transferzeit", "", "");
            CreateOffers(299, "Rügen", 3, "Zum goldenen Strand", "Fähre", 5, "Vollpension", CategoryEnum.Beach, "images/Beach/ruegen.jpg", false, "Mit Meerblick", "", "");
            //Wandern
            CreateOffers(110, "Adeche", 3, "Bungalow Rivière", "eigene Anreise", 4, "Halbpension", CategoryEnum.Wander, "images/Wander/adeche.jpg", false, "Optionale Kanutour", "", "");
            CreateOffers(130, "Andalusien", 4, "Hotel por toro", "eigene Anreise", 4, "Halbpension", CategoryEnum.Wander, "images/Wander/andalusien.jpg", false, "Stierschau möglich", "", "");
            CreateOffers(459, "Badlands Nationalpark", 3, "Hotel of Buffalo", "Flugreise", 6, "Halbpension", CategoryEnum.Wander, "images/Wander/badlandsnationalpark.jpg", false, "", "", "");
            CreateOffers(109, "Bayrischer Wald", 3, "Hotel zum Bayer", "eigene Anreise", 3, "Halbpension", CategoryEnum.Wander, "images/Wander/bayrischerwald.jpg", false, "inkl. Bierverkostung", "exkl. Besuch bei Bayrisch Bräu", "");
            CreateOffers(399, "China", 2, "Best Eastern", "Flugreise", 8, "Vollpension", CategoryEnum.Wander, "images/Wander/china.jpg", false, "Bergsteigen ganz traditionell", "exkl. Besuch der Großen Mauer", "");
            CreateOffers(499, "Grand Canyon", 3, "Best Sight Hotel", "Flugreise", 6, "Halbpension", CategoryEnum.Wander, "images/Wander/grandcanyon.jpg", true, "inkl. Ausflug zum Canyon Skywalk", "", "");
            CreateOffers(219, "Irland", 4, "Whisky Palace", "Flugreise", 5, "Vollpension", CategoryEnum.Wander, "images/Wander/ireland.jpg", false, "inkl. Whisky-Tasting", "", "");
            CreateOffers(89, "Pfälzerwald", 3, "Hochwald Hotel", "eigene Anreise", 3, "Halbpension", CategoryEnum.Wander, "images/Wander/plälzerwald.jpg", false, "", "", "");
            CreateOffers(599, "Yellowstone", 4, "Big Stone Hotel", "Flugreise", 7, "Vollpension", CategoryEnum.Wander, "images/Wander/yellowstone.jpg", true, "inkl. Wellnessangebot", "", "");
            //Ski
            CreateOffers(1499, "Banff", 4, "Banff Lodge", "Flugreise", 10, "Halbpension", CategoryEnum.Ski, "images/Snow/banff.jpg", false, "inkl. Leihski", "Heliskiing", "");
            CreateOffers(499, "Ischgl", 3, "Appartment Gerda", "eigene Anreise", 6, "Selbstverpflegung", CategoryEnum.Ski, "images/Snow/ischgl.jpg", false, "Zimmer mit kostenlosem Pay-TV", "Skiraum", "Familienfreundlich");
            CreateOffers(399, "Kitzbühl", 5, "Hotel zur Streif", "eigene Anreise", 5, "Frühstück", CategoryEnum.Ski, "images/Snow/kitzbuehl.jpg", false, "inkl. Skipass", "Parkplatz", "Wilkommenscocktail");
            CreateOffers(199, "Les Arcs", 3, "Gasthof Claude", "Busreise", 4, "Halbpension", CategoryEnum.Ski, "images/Snow/lesarcs.jpg", false, "VIP Reisebus", "", "");
            CreateOffers(899, "Schladming", 5, "Hotel Planei", "eigen Anreise", 8, "Vollpension", CategoryEnum.Ski, "images/Snow/schladming.jpg", false, "inkl. Skipass", "inkl. Saunabenutzung", "Parkplatz");
            CreateOffers(399, "Verbier", 4, "Persion Cuche", "Busreise", 5, "Halbpension", CategoryEnum.Ski, "images/Snow/verbier.jpg", false, "exkl. Skipass", "5-Sterne Reisebus", "");
            CreateOffers(650, "Zinal", 4, "Hotel Matterhornblick", "eigene Anreise", 7, "Halbpension", CategoryEnum.Ski, "images/Snow/zinal.jpg", false, "inkl. Parkplatz in der Tiefgarage", "5 Gehminuten von der Talstation entfernt", "");
            CreateOffers(1999, "Whistler", 5, "Five Seasons Resort", "Flugreise", 14, "Halbpension", CategoryEnum.Ski, "images/Snow/whistler.jpg", false, "inkl. Skipass", "inkl. Leihski", "Heliskiing zubuchbar");
            CreateOffers(499, "Zermatt", 3, "Garni Hotel Emma", "Zug", 5, "Frühstück", CategoryEnum.Ski, "images/Snow/zermatt.jpg", false, "Herrlicher Blick auf das Matterhorn", "Skibus-Haltestelle direkt vor dem Haus", "");
        }

        /// <summary>
        /// Deletes all entries in database
        /// </summary>
        /// <param name="context">db context</param>
        public void FlushDbData(Model1Container context)
        {
            if (context.Database.Exists())
            {
                context.Database.Delete();
            }
            context.Database.CreateIfNotExists();
        }

        static void Main(string[] args)
        {
            CreateDbData dbf = new CreateDbData();
            using (var context = new Model1Container())
            {
                dbf.FlushDbData(context);
                dbf.CreateCountryEntries(context);
            }
            dbf.CreateMyOffers();
            Console.WriteLine("-- Press any key to exit --");
        }
    }
}
