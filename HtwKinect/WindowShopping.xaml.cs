﻿using System.Windows;
using PeopleDetector;
using HtwKinect.StateViews;
using System.Windows.Controls;
using System;
using System.Windows.Input;
using Database;
using Database.DAO;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für FrameWindow.xaml
    /// </summary>
    public partial class FrameWindow : Window
    {
        #region debug keys
        private bool _debugOnlyScreen2;
        private bool _debugOnlyScreen3;
        private bool _debugOnlyScreen1;
        #endregion debug keys

        private PeoplePositionDetector _peopleDetector;
        private ScreenMode _currentScreen = ScreenMode.Splash;


        // vars for Buffer
        private readonly ScreenMode[] _screenstatusarray;
        private int buffersize = 20; //frames,
        private int _bufferIterator;


        public enum ScreenMode
        {
            Splash,
            Walk,
            MainScreen,
            Unknown
        }

        private LoopScreen _mainWindow;
        private StateViews.SplashScreen _sscreen;
        private WalkScreen _walkScreen;

        private int _walkingPeople = 0;
        private int _positionOnlyPeople = 0;
        private int _trackedPeople = 0;
        private int _lookingPeople = 0;
        private int _standingPeople = 0;


        public FrameWindow()
        {
            _screenstatusarray = new ScreenMode[buffersize];
            InitializeComponent();
            StartSplashScreen();           
        }

        private void ChangeScreen()
        {

            #region debug keys effect
            if (_debugOnlyScreen2)
            {
                if (_currentScreen != ScreenMode.Walk)
                {
                    StartWalkScreen();
                }
                return;
            }

            if (_debugOnlyScreen3)
            {
                if (_currentScreen != ScreenMode.MainScreen)
                {
                    StartMainScreen();
                }
                return;
            }

            if (_debugOnlyScreen1)
            {
                if (_currentScreen != ScreenMode.Splash)
                {
                    StartSplashScreen();
                }
                return;
            }
            #endregion debug keys effect


            _walkingPeople = _peopleDetector.GetWalkingPeople().Count;
            _positionOnlyPeople = _peopleDetector.GetPositionOnlyPeople().Count;
            _trackedPeople = _peopleDetector.GetTrackedPeople().Count;
            _lookingPeople = _peopleDetector.GetLookingPeople().Count;
            _standingPeople = _peopleDetector.GetStayingPeople().Count;

            if (_currentScreen == ScreenMode.MainScreen && _mainWindow.IsGame()) {
                return;
            }

            if (_positionOnlyPeople == 0 && _trackedPeople == 0 ) //Zustand 1
            {
                AddToBuffer(ScreenMode.Splash);
                if (_currentScreen != ScreenMode.Splash && MostBufferedScreen() == ScreenMode.Splash)
                {
                    StartSplashScreen();
                }
            }
            else  // Zustand 2-4
            {
                if (_standingPeople == 0 &&  _walkingPeople != 0 && _lookingPeople == 0 ) // Zustand 2
                {
                    AddToBuffer(ScreenMode.Walk);
                    if (_currentScreen != ScreenMode.Walk && MostBufferedScreen() == ScreenMode.Walk)
                    {
                        StartWalkScreen();
                    }
                }
                else if (_standingPeople != 0 && _lookingPeople != 0) // Zustand 3
                {
                    AddToBuffer(ScreenMode.MainScreen);
                    if (_currentScreen != ScreenMode.MainScreen && MostBufferedScreen() == ScreenMode.MainScreen)
                    {
                        StartMainScreen();
                    }
                }
            }
        }


        private void AddToBuffer(ScreenMode lastdetected) 
        {
            //loop overwrite
            _bufferIterator = (_bufferIterator == _screenstatusarray.Length) ? 0 : _bufferIterator;
            _screenstatusarray[_bufferIterator] = lastdetected;
            _bufferIterator++;
        }

        private ScreenMode MostBufferedScreen()
        {
            // double Counter
            int splashCounter = 0;
            int walkcounter = 0;
            int maincounter = 0;

            foreach (ScreenMode currentEntry in _screenstatusarray)
            {
                switch (currentEntry)
                {
                    case ScreenMode.Splash:
                        splashCounter++;
                        break;
                    case ScreenMode.Walk:
                        walkcounter++;
                        break;
                    case ScreenMode.MainScreen:
                        maincounter++;
                        break;
                    default:
                        break;
                }
            }


            if (splashCounter > _screenstatusarray.Length / 2)
            {
                return ScreenMode.Splash;
            }
            else if (walkcounter > _screenstatusarray.Length / 2)
            {
                return ScreenMode.Walk;
            }
            else if (maincounter > _screenstatusarray.Length / 2)
            {
                return ScreenMode.MainScreen;
            }

            //Default
            return ScreenMode.Unknown;
        }


        private void StartSplashScreen() 
        {
            RemoveOldScreen();
            if (_sscreen == null) 
            { 
                _sscreen = new StateViews.SplashScreen();       
            }
            _sscreen.StartDisplay(StopLastScreenAndGetLastTravel());
            _currentScreen = ScreenMode.Splash;
            _sscreen.StartNewOfferTimer(Properties.Settings.Default.PictureChangeIntervallMS); 
            Grid.SetRow(_sscreen, 1);
            GridX.Children.Add(_sscreen);
        }

        private void StartWalkScreen()
        {
            RemoveOldScreen();   
            if (_walkScreen == null) { _walkScreen = new WalkScreen(); }
            _walkScreen.StartDisplay(StopLastScreenAndGetLastTravel());
            _currentScreen = ScreenMode.Walk;
            Grid.SetRow(_walkScreen, 1);
            GridX.Children.Add(_walkScreen);
        }

        private void StartMainScreen()
        {
            RemoveOldScreen();
            _mainWindow = new LoopScreen(); //bad perf, but less problems
           // if (_mainWindow == null) { _mainWindow = new LoopScreen(); }
            _mainWindow.StartDisplay(StopLastScreenAndGetLastTravel());
            _currentScreen = ScreenMode.MainScreen;
            Grid.SetRow(_mainWindow, 1);
            GridX.Children.Add(_mainWindow);
        }

        /**
         * returns last Offer, if nothing available Random Top-TravelOffer 
         */
        private TravelOffer StopLastScreenAndGetLastTravel()
        {
            switch (_currentScreen)
            {
                case ScreenMode.Splash:
                    return _sscreen.StopDisplay();
                case ScreenMode.Walk:
                    return _walkScreen.StopDisplay();
                case ScreenMode.MainScreen:
                    return _mainWindow.StopDisplay();
                default:
                    return new TravelOfferDao().SelectRandomTopOffer();
            }
        }

        private void RemoveOldScreen()
        {
            if (GridX.Children.Count > 0)
            {
                GridX.Children.RemoveAt(0);
            }
        }


        /*Tastensteuerung */
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                if (e.SystemKey == Key.F4)
                {
                    Application.Current.Shutdown();
                    e.Handled = true;
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            Application.Current.Shutdown();
                            e.Handled = true;
                            break;
                        #region debug keys trigger
                        case Key.Space:
                            _debugOnlyScreen2 = false;
                            _debugOnlyScreen3 = !_debugOnlyScreen3;
                            _debugOnlyScreen1 = false;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        case Key.D2:
                            _debugOnlyScreen3 = false;
                            _debugOnlyScreen1 = false;
                            _debugOnlyScreen2 = !_debugOnlyScreen2;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        case Key.D1:
                            _debugOnlyScreen2 = false;
                            _debugOnlyScreen3 = false;
                            _debugOnlyScreen1 = !_debugOnlyScreen1;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        case Key.D3:
                            _debugOnlyScreen2 = false;
                            _debugOnlyScreen1 = false;
                            _debugOnlyScreen3 = !_debugOnlyScreen3;
                            ChangeScreen();
                            e.Handled = true;
                            break;
                        #endregion debug keys trigger
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception in WindowShopping: "+exc);
            }
            if (e.Handled == false && _currentScreen==ScreenMode.MainScreen && _mainWindow!=null) 
            {
              _mainWindow.DelegateKeyEvent(e);
            }

        }

        #region PeopleDetector and Window start exit

        /// <summary>
        /// Wird nach dem Laden des Fensters aufgerufen.
        /// </summary>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _peopleDetector = new PeoplePositionDetector();
            try
            {
                KinectHelper kh = KinectHelper.Instance;
                kh.ReadyEvent += PeopleDetectorSkeletonEvent;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        /// <summary>
        /// Wird beim Schliessen des Fensters aufgerufen.
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Console.WriteLine("Joke: Fenster Klose");
        }

        /// <summary>
        /// Event Skeleton für PeopleDetector.
        /// </summary>
        private void PeopleDetectorSkeletonEvent(object sender, EventArgs e)
        {
            _peopleDetector.TrackSkeletons(KinectHelper.Instance.Skeletons);

            /*if (_mainWindow == null)
            {
                OutputLabelX.Content =
                    "Erkannt:" + _peopleDetector.GetPositionOnlyPeople().Count +
                    " Tracked:" + _peopleDetector.GetTrackedPeople().Count +
                    " Walking:" + _peopleDetector.GetWalkingPeople().Count +
                    " Standing:" + _peopleDetector.GetStayingPeople().Count +
                    " Looking:" + _peopleDetector.GetLookingPeople().Count;
            }
            else
            {
                OutputLabelX.Content =
                    "Erkannt:" + _peopleDetector.GetPositionOnlyPeople().Count +
                    " Tracked:" + _peopleDetector.GetTrackedPeople().Count +
                    " Walking:" + _peopleDetector.GetWalkingPeople().Count +
                    " Standing:" + _peopleDetector.GetStayingPeople().Count +
                    " Looking:" + _peopleDetector.GetLookingPeople().Count +
                    " Gender:" + _mainWindow.Gender;
            }*/
            ChangeScreen();
        }
        #endregion PeopleDetector and Window start exit
    }
}
