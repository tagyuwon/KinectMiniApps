﻿using System.Collections.Generic;
using HandDetection;
using LoopList;
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Kinect;

namespace HtwKinect
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Point? _oldMovePoint;
        private bool _doDrag;
        private bool _waitForTextList;
        private bool _mouseIsUp;
        private KinectHelper _kinectHelper;
        private KinectProjectUiBuilder _kinectProjectUiBuilder;

        private readonly List<int> _savedDirections = new List<int>();
        private bool _dragDirectionIsObvious;

        
        private HandTracker _handTracker;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                InitList();
                InitKinect();
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void InitList()
        {
            MyLoopList.SetAutoDragOffset(0.20);
            MyLoopList.SetDuration(new Duration(new TimeSpan(3000000))); //300m
            MyLoopList.Scrolled += MyLoopListOnScrolled;
            MyTextLoopList.Scrolled += MyTextLoopList_Scrolled;
            MyTextLoopList.SetFontSize(36);
            MyTextLoopList.SetFontFamily("Miriam Fixed");
            MyTextLoopList.SetDuration(new Duration(new TimeSpan(5500000)));
            LoadPictures(new LocalPictureUiLoader());
        }

        private void LoadPictures(IUiLoader uiLoader)
        {
            _kinectProjectUiBuilder = new KinectProjectUiBuilder(MyLoopList, MyTextLoopList);
            uiLoader.LoadElementsIntoList(_kinectProjectUiBuilder);
        }

        private void InitKinect()
        {
            _handTracker = new HandTracker();
            _kinectHelper = KinectHelper.GetInstance();
            _kinectHelper.AllFramesDispatchedEvent += (s, _) => HelperReady();
        }

        /*Callback fur ein fertiges Frame vom Kinect-Sensor*/
        private void HelperReady()
        {
            Skeleton skeleton = _kinectHelper.GetFixedSkeleton();
            ProcessSkeleton(skeleton);
        }

        private void ProcessSkeleton(Skeleton skeleton)
        {
            if (skeleton == null)
            {
                return;
            }

            HandStatus handStatus = _handTracker.GetBufferedHandStatus(_kinectHelper.GetDepthImagePixels(),
                                                                       skeleton.Joints[JointType.HandRight],
                                                                       _kinectHelper.GetSensor(),
                                                                       _kinectHelper.GetDepthImageFrame().Format);
            
            switch (handStatus)
            {
                case HandStatus.Closed:
                    myLoopList_MouseUp_1(null, null);
                    break;
                case HandStatus.Opened:
                    myLoopList_MouseDown_1(null, null);
                    break;
                default:
                    return;
            }
            ColorImagePoint cp = _kinectHelper.GetSensor().CoordinateMapper.MapSkeletonPointToColorPoint(skeleton.Joints[JointType.HandRight].Position, _kinectHelper.GetColorImageFrame().Format);

            Point currentPoint = new Point(cp.X*6, cp.Y*6);
            Drag(currentPoint);
        }

       
        /*Erst wenn die Scrollanimation der TextLoopList beendet ist, darf die LoopList weiterscrollen (vertical).*/
        private void MyTextLoopList_Scrolled(object sender, EventArgs e)
        {
            _waitForTextList = false;
            if (!_mouseIsUp)
                _doDrag = true;
        }

        /*Wenn die LoopList vertical gescrollt wurde, wird die TextLoopList gescrollt.*/
        private void MyLoopListOnScrolled(object sender, EventArgs e)
        {
            if (e != null)
            {
                switch (((LoopListArgs) e).GetDirection())
                {
                    case Direction.Top:
                        _waitForTextList = MyTextLoopList.Anim(true);
                        break;
                    case Direction.Down:
                        _waitForTextList = MyTextLoopList.Anim(false);
                        break;
                }
                ResetDragDirectionObvious();
                if (!_mouseIsUp)
                    _doDrag = true;
            }
        }

        private void Drag(Point currentPos)
        {
            try
            {
                if (!_doDrag) goto exit;
                if (!_oldMovePoint.HasValue)
                    _oldMovePoint = currentPos;
                if (Math.Abs(_oldMovePoint.Value.X - currentPos.X) < 0.000000001 &&
                    Math.Abs(_oldMovePoint.Value.Y - currentPos.Y) < 0.000000001) goto exit; //keine Bewegung?


                int xDistance = (int) (currentPos.X - _oldMovePoint.Value.X);
                int yDistance = (int) (currentPos.Y - _oldMovePoint.Value.Y);

                int dragDirection = Math.Abs(xDistance) >= Math.Abs(yDistance) ? 1 : 2;
                if (!_dragDirectionIsObvious)
                {
                    if (_savedDirections.Count < 4)
                    {
                        _savedDirections.Add(dragDirection);
                        goto exit;
                    }
                    int xCount = 0;
                    int yCount = 0;
                    foreach (int dir in _savedDirections)
                    {
                        if (dir == 1)
                            xCount++;
                        else if (dir == 2)
                            yCount++;
                    }
                    int greater = Math.Max(xCount, yCount);
                    int lower = Math.Min(xCount, yCount);
                    if (lower/(double) greater < 0.15) //x- und y-Entwicklung unterscheiden sich deutlich.
                    {
                        _dragDirectionIsObvious = true;
                        dragDirection = greater == xCount ? 1 : 2;
                        KinectVibratingRectangle.Visibility = Visibility.Collapsed;
                    }
                    _savedDirections.Clear();
                    if (!_dragDirectionIsObvious)
                    {
                        KinectVibratingRectangle.Visibility = Visibility.Visible;
                        goto exit;
                    }
                }

                bool mayDragOn = false;
                if (dragDirection == 1)
                {
                    mayDragOn = MyLoopList.HDrag(xDistance);
                }
                if (dragDirection == 2)
                {
                    if (!_waitForTextList)
                        mayDragOn = MyLoopList.VDrag(yDistance);
                }
                if (!mayDragOn) _doDrag = false;
                exit:
                _oldMovePoint = currentPos;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            Drag(e.GetPosition(MyLoopList));
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            try
            {
                KinectFocusedRectangle.Visibility = Visibility.Collapsed;
                _mouseIsUp = true;
                ResetDragDirectionObvious();
                
                _doDrag = false;
                _oldMovePoint = null;
                MyLoopList.AnimBack(); //zurueckspringen des Bildes
                
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        private void ResetDragDirectionObvious()
        {
            _dragDirectionIsObvious = false;
            KinectVibratingRectangle.Visibility = Visibility.Collapsed;
            _savedDirections.Clear();
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            _mouseIsUp = false;
            _doDrag = true;
            KinectFocusedRectangle.Visibility = Visibility.Visible;
        }

        /*Tastensteuerung der LoopList*/
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Left:
                        MyLoopList.AnimH(true);
                        break;
                    case Key.Right:
                        MyLoopList.AnimH(false);
                        break;
                    case Key.Up:
                        if (!_waitForTextList)
                            MyLoopList.AnimV(true);
                        break;
                    case Key.Down:
                        if (!_waitForTextList)
                            MyLoopList.AnimV(false);
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                }

                e.Handled = true;
            }
            catch (Exception exc)
            {
                ExceptionTextBlock.Text = exc.Message + "\n\r" + exc.InnerException;
            }
        }

        /*MouseLeave wird wie MouseUp behandelt*/
        private void myLoopList_MouseLeave_1(object sender, MouseEventArgs e)
        {
            myLoopList_MouseUp_1(null, null);
        }

    }
}