﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AccessoryLib;
using Database;
using Database.DAO;

namespace HtwKinect
{
    /// <summary>
    /// Diese Klasse lädt lokale Testbilder in die LoopList.
    /// </summary>
    class LocalPictureUiLoader : IUiLoader
    {
        public void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder)
        {
            var offerDao = new TravelOfferDao();
            //string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images\Top");
            List<TravelOffer> dbList = offerDao.SelectAllTopOffers();
            List<FrameworkElement> list = new List<FrameworkElement> ();
            foreach (var offer in dbList)
            {
                Grid grid = new Grid();
                BuildBackground(grid, offer.ImgPath);
                BuildInfoBox(grid, offer);
                list.Add(grid);
            }
            try
            {
                MiniGame.MiniGameControl mg = new MiniGame.MiniGameControl();
                mg.Start(KinectHelper.Instance.Sensor);
                KinectHelper.Instance.ReadyEvent += (sender, _) => Instance_ReadyEvent(mg);
                list.Add(mg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            kinectProjectUiBuilder.AddRow("Top", list);

            //todo maybe ask db for categories and not enum
            foreach (CategoryEnum category in Enum.GetValues(typeof(CategoryEnum)).Cast<CategoryEnum>()) 
            {
                list = new List<FrameworkElement>();
                dbList = offerDao.SelectOfferyByCategory(category);
                foreach (var offer in dbList)
                {
                    Grid grid = new Grid();
                    BuildBackground(grid, offer.ImgPath);
                    BuildInfoBox(grid, offer);
                    list.Add(grid);
                }
                kinectProjectUiBuilder.AddRow(dbList.First().Category.CategoryName, list);
            }
        }

        void Instance_ReadyEvent(MiniGame.MiniGameControl mg)
        {
            mg.MinigameSkeletonEvent(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.DepthImagePixels, KinectHelper.Instance.ColorPixels);
            KinectHelper.Instance.SetTransform(mg);
        }

        #region BackgroundPicture
        private void BuildBackground(Grid grid, string imgPath)
        {
            try
            {
                var img = new Image { Source = new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)), Stretch = Stretch.Fill };
                grid.Children.Add(img);
            }
            catch
            {
                Console.WriteLine("can't load or display the background image: " + imgPath);
            }
        }
        #endregion

        #region InfoBox
        private void BuildInfoBox(Grid grid, TravelOffer offer)
        {
            try
            {
                var infoBanner = new InfoBanner.InfoBanner();
                infoBanner.HorizontalAlignment = HorizontalAlignment.Left;
                infoBanner.Start(offer);
                grid.Children.Add(infoBanner);
            }
            catch
            {
                Console.WriteLine("Error in LocalPictureUiLoader");
            }
        }

        #endregion

    }
}
