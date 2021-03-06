﻿using Microsoft.Kinect;
using SkyBiometry.Client.FC;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenderDetector
{
    /// <summary>
    /// Klasse zur Alterbestimmung.
    /// </summary>
    public partial class GenderDetectorControl
    {
        private KinectSensor _kinectSensor;
        private WriteableBitmap _colorBitmap;
        private FCClient _client;
        private FCResult _result;
        private Skeleton _activeSkeleton;
        private const int CutRange = 125;

        public String Gender { get; set; }
        public String Confidence { get; set; }

        public GenderDetectorControl()
        {
            InitializeComponent();
        }

        public void Start(KinectSensor sensor)
        {
            _kinectSensor = sensor;
        }

        /// <summary>
        /// EventHandler zum Zeichnen des Bildes.
        /// </summary>
        public void GenderCheck(Skeleton skeleton, byte[] colorImagePoints)
        {
            if (skeleton == null || colorImagePoints == null) return;
            // Write the pixel data into our bitmap
            _colorBitmap = new WriteableBitmap(_kinectSensor.ColorStream.FrameWidth, _kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            _colorBitmap.WritePixels(
                new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
                colorImagePoints,
                _colorBitmap.PixelWidth * sizeof(int),
                0);
            _activeSkeleton = skeleton;

            if (CheckForInternetConnection())
            {
                GenderCheckCutOut();
            }
        }

        /// <summary>
        /// Hauptfunction zur Altersbestimmung.
        /// </summary>
        private void GenderCheckCutOut()
        {
            new Thread((ThreadStart)delegate
            {
                // Rest Service initaliesieren
                InitializeService();

                // Bild speichern
                String path = "";
                bool imagefailed = false;
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CroppedBitmap colorBitmap = CropBitmap(_colorBitmap);
                    if (colorBitmap == null)
                    {
                        path = SaveScreenshot(_colorBitmap);
                    }
                    else
                    {
                        path = SaveScreenshot(colorBitmap);
                    }
                }));

                // Warten bis Bild gespeichert wurde
                int currentminute = System.DateTime.Now.Minute;
                int diff = 0;
                while (path == "" && !imagefailed) {
                    diff = System.DateTime.Now.Minute - currentminute;
                    if (diff > 2 || diff < -2) { // wenns länger als 1 min braucht brich ab
                        imagefailed = true;
                    }
                }
                //Console.WriteLine(System.DateTime.Now.Minute);
                // Auswertung des Bildes
                if (!imagefailed)
                {
                    CalculateGender(path);
                }

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    // Attribute setzen
                    SetAttributes();
                    // Bild wieder löschen
                    if (!imagefailed)
                    {
                        File.Delete(path);
                    }
                }));
            }).Start();
        }

        /// <summary>
        /// Initialisiert den Rest Service.
        /// </summary>
        private void InitializeService()
        {
                _client = new FCClient("5f228f0a0ce14e86a7c901f62ca5a569", "1231e9f2e95d4d90adf436e3de20f0f6");
                _result = _client.Account.EndAuthenticate(_client.Account.BeginAuthenticate(null, null));
        }

        /// <summary>
        /// Schneidet den Kopf aus der colorBitmap wenn eine Person erkannt wird.
        /// </summary>
        private CroppedBitmap CropBitmap(WriteableBitmap colorBitmap) 
        {
            // Ersten Player selektieren
            if (_activeSkeleton != null)
            {
                // Punkte des Kopfes auf ColorPoints mappen
                var point = _kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(_activeSkeleton.Joints[JointType.Head].Position, _kinectSensor.ColorStream.Format);
                    
                // Überprüfung das nicht außerhalb des Bildes ausgenschnitten wird
                if ((int)point.X - CutRange <= 0 || (int)point.X + CutRange >= _kinectSensor.ColorStream.FrameWidth || (int)point.Y - CutRange <= 0 || (int)point.Y + CutRange >= _kinectSensor.ColorStream.FrameHeight)
                    return null;
                // Array für auszuschneidendes Bild initialisierne
                Int32Rect cropRect =
                    new Int32Rect((int)point.X - CutRange, (int)point.Y - CutRange, CutRange * 2, CutRange * 2);

                // Neues Bild erstellen und zurückliefern
                return new CroppedBitmap(_colorBitmap, cropRect);
            }
            return null;
        }

        /// <summary>
        /// Speichert den Screenshort zur späteren Verwendung.
        /// </summary>
        private String SaveScreenshot(BitmapSource colorBitmap)
        {
            // Erzeugt den Encoder zum Speichern eines JPG
            BitmapEncoder encoder = new JpegBitmapEncoder();

            // Schreibt die colorBitmap in den Encoder
            encoder.Frames.Add(BitmapFrame.Create(colorBitmap));

            // Speicherpfad erzeugen
            String time = System.DateTime.Now.ToFileTime().ToString();//System.DateTime.Now.ToString("hh'-'mm'-'ss");
            String myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            String path = System.IO.Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // Speichern des Bildes
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch {
                Console.WriteLine("Gender Image failed");
            }
            return path;
        }

        /// <summary>
        /// Sendet das Bild an den Rest Service und speichert das Ergebnis.
        /// </summary>
        private void CalculateGender(String path)
        {
            Stream stream = System.IO.File.OpenRead(path);
            _result = _client.Faces.EndDetect(_client.Faces.BeginDetect(null, new Stream[] { stream }, Detector.Normal, Attributes.All, null, null));
            stream.Close();
        }

        /// <summary>
        /// Setzt die Attribute und die WPF Elemente nach einem Request.
        /// </summary>
        private void SetAttributes()
        {
            if (_result != null && _result.Photos != null)
            {

                if (_result.Photos[0].Tags != null && _result.Photos[0].Tags.Count == 0)
                {
                    Gender = "No face tracked";
                }
                else if (_result.Photos[0].Tags != null)
                {
                    Gender = _result.Photos[0].Tags[0].Attributes.Gender.Value + "";
                    Confidence = _result.Photos[0].Tags[0].Attributes.Gender.Confidence + "";
                }
            }
            else
            {
                Gender = "No Result";
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
