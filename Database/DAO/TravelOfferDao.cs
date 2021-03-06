﻿using Database.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAO
{
    public class TravelOfferDao
    {

        private TravelOffer _offer;

        private bool IsNew()
        {
            return _offer.OfferId == 0;
        }

        private void Insert()
        {
            using (var con = new Model1Container())
            {
                con.TravelOfferSet.Add(_offer);
                con.SaveChanges();
            }
        }

        private void Update()
        {
            using (var con = new Model1Container())
            {

                var offerEntity = con.TravelOfferSet.Single(o => o.OfferId == _offer.OfferId);
                con.Entry(offerEntity).CurrentValues.SetValues(_offer);
                con.SaveChanges();
            } 
        }

        public List<TravelOffer> SelectOfferyByCategory(CategoryEnum category)
        {
            try
            {
                using (var con = new Model1Container())
                {
                    var list = (from offer in con.TravelOfferSet
                                               .Include("Category")
                                               .Include("ExtendedInformation")
                                        where offer.CategoryId == (int) category
                                        select offer).ToList();
                    if (list.Count <= 0)
                        throw new Exception("No top offer found");
                    return list;
                }
            }
            catch (Exception)
            {
                return new List<TravelOffer>() { CreateDefaultObject() };
            }
        }

        public List<TravelOffer> SelectAllOffers()
        {
            using (var con = new Model1Container())
            {

                return (from offer in con.TravelOfferSet.Include("Category")
                        select offer).ToList();
            } 
        }

        public TravelOffer SelectById(int offerId)
        {
            try
            {
                using (var con = new Model1Container())
                {

                    var obj = (from offer in con.TravelOfferSet
                                             .Include("Category")
                                             .Include("ExtendedInformation")
                               where offerId == offer.OfferId
                               select offer).FirstOrDefault();
                    if (obj == null)
                        throw new Exception("No entry found. Wrong Id");
                    return obj;
                }
            }
            catch (Exception)
            {
                return CreateDefaultObject();
            }
        }

        public TravelOffer SelectRandomTopOffer()
        {
            var list = SelectAllTopOffers();
            return list.ElementAt(Helper.GetRandomInteger(0, list.Count - 1));
        }

        public List<TravelOffer> SelectAllTopOffers()
        {
            try
            {
                using (var con = new Model1Container())
                {
                    var topOfferlist = (from offer in con.TravelOfferSet
                                               .Include("Category")
                                               .Include("ExtendedInformation")
                                        where offer.TopOffer == true
                                        select offer).ToList();
                    if (topOfferlist.Count <= 0)
                        throw new Exception("No top offer found");
                    return topOfferlist;
                }
            }
            catch (Exception)
            {
                return new List<TravelOffer>() {CreateDefaultObject()};
            }
        } 

        private TravelOffer CreateDefaultObject()
        {
            var exInf = new Collection<ExtendedInformation>
                            {new ExtendedInformation() {Information = "please fill database"}};
            return new TravelOffer()
                       {
                           Category = new Category(){CategoryName = "No Db data", CategoryId = 0},
                           BoardType = "no data",
                           CategoryId = 0,
                           DayCount = 0,
                           HotelName = "no data",
                           HotelRating = 0,
                           OfferId = 0,
                           Place = "no data",
                           PricePerPerson = 123.4,
                           TravelType = "no data",
                           ExtendedInformation = exInf,
                           TopOffer = false
                       };
        }

        public void Save(TravelOffer offer)
        {
            _offer = offer;
            if (IsNew())
                Insert();
            else
                Update();
        }
    }
}
