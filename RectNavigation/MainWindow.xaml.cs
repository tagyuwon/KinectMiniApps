﻿using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Linq;
using System;
using System.Windows.Media.Animation;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{


    public partial class MainWindow
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor _sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>


        private bool _isInInnerRect;
        private bool _wasInLastFrameInInnerRect;
        private bool _isInOuterRect;
        private bool _wasInLastFrameInOuterRect;

        private Point _handRightPoint = new Point(0, 0);
        private Rect _innerRect;
        private Rect _outerRect;

        private bool _wasInInnerRect;

        // Rectangle Fade out variables
        private const int RectFadeOutTimer = 2000; // Miliseconds   -> Time when Fade-out animation starts
        private long _enterInnerRectTimestamp;
        private bool _isAnimating;


        public void Start(KinectSensor sensor)
        {
            _sensor = sensor;
        }

        private void GestureRecognition(Skeleton[] skeletons)
        {
            if (skeletons.Count(t => t.TrackingState == SkeletonTrackingState.Tracked) >= 1)
            {
                Skeleton skel = skeletons.First(s => s.TrackingState == SkeletonTrackingState.Tracked);
                Joint handRight = skel.Joints[JointType.HandRight];
                
                _innerRect = GetInnerRect(skel);
                _outerRect = GetOuterRect(_innerRect);
                TransformRectangles();
                TransformHand();
                _handRightPoint = SkeletonPointToScreen(handRight.Position);

                _isInInnerRect = _innerRect.Contains(_handRightPoint);
                _isInOuterRect = _outerRect.Contains(_handRightPoint);

                // Inneres Rechteck wurde betreten oder verlassen
                if(_isInInnerRect != _wasInLastFrameInInnerRect)
                {
                    // Inneres Rechteck wurde betreten
                    if (_isInInnerRect)
                    {
                        // Fade-out: Save timestamp when enter the inner rect
                        _enterInnerRectTimestamp = getTimeStamp();
                        _wasInInnerRect = true;
                    }
                    // Inneres Rechteck wurde verlassen
                    else
                    {
                        
                    }
                    _wasInLastFrameInInnerRect = _isInInnerRect;
                }

                // Aeusseres Rechteck wurde betreten oder verlassen
                if (_isInOuterRect != _wasInLastFrameInOuterRect)
                {
                    // Aeusseres Rechteck wurde betreten
                    if (_isInOuterRect)
                    {
                    }
                    // Aeusseres Rechteck wurde verlassen
                    else
                    {
                        if (_wasInInnerRect)
                        {
                            if (_handRightPoint.X > _outerRect.TopRight.X) //leave right
                            {
                                SwipeRight();
                            }
                            else if (_handRightPoint.X < _outerRect.TopLeft.X) //leave left
                            {
                                SwipeLeft();
                            }
                            else if (_handRightPoint.Y > _outerRect.BottomLeft.Y) //leave bottom
                            {
                                SwipeDown();
                            }
                            else if (_handRightPoint.Y < _outerRect.TopLeft.Y) //leave top
                            {
                                SwipeUp();
                            }
                            _wasInInnerRect = false;
                        }
                    }
                }
                _wasInLastFrameInOuterRect = _isInOuterRect;

                if (_isInOuterRect && !_isInInnerRect && _wasInInnerRect)
                {
                    double swipeLeft = GetPercentageSwipeLeft(_handRightPoint);
                    double swipeRight = GetPercentageSwipeRight(_handRightPoint);
                    double swipeTop = GetPercentageSwipeTop(_handRightPoint);
                    double swipeBottom = GetPercentageSwipeBottom(_handRightPoint);
                    if(swipeLeft >= 0)
                    {
                    }
                    if (swipeRight >= 0)
                    {
                    }
                    if (swipeTop >= 0)
                    {
                    }
                    if (swipeBottom >= 0)
                    {
                    }
                }

                //Fade-out: If true, start fade animation
                if (getTimeStamp() - _enterInnerRectTimestamp > RectFadeOutTimer && _isInInnerRect)
                {
                    _wasInInnerRect = false;
                }
            }
        }

        private long getTimeStamp()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private Rect GetInnerRect(Skeleton skeleton)
        {
            Point spine = SkeletonPointToScreen(skeleton.Joints[JointType.Spine].Position);
            Point hipRight = SkeletonPointToScreen(skeleton.Joints[JointType.HipRight].Position);
            Point shoulderCenter = SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderCenter].Position);
            double x = hipRight.X;
            double y = shoulderCenter.Y;

            // Rechteck verschieben
            const int offsetX = 20;
            const int offsetY = 0;

            // inneres Rechteck verkleinern
            double height = Math.Abs(spine.Y - y);

            double width = height;

            return new Rect(x  + offsetX, y + offsetY, width, height);
        }

        private Rect GetOuterRect(Rect innerRect)
        {
            const double border = 30;
            double x = innerRect.X - border;
            double y = innerRect.Y - border;
            double width = innerRect.Width + border * 2;
            double height = innerRect.Height + border * 2;
            return new Rect(x, y, width, height);
        }

        private void AnimateHandPoint(Point from, Point to)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(1));
            DoubleAnimation animationX = new DoubleAnimation(from.X, to.X + 75, duration);
            DoubleAnimation animationY = new DoubleAnimation(from.Y, to.Y, duration);
            animationX.Completed += AnimationCompleted;
            TranslateTransform trans = new TranslateTransform();
            Hand.RenderTransform = trans;
            trans.BeginAnimation(TranslateTransform.XProperty, animationX);
            trans.BeginAnimation(TranslateTransform.YProperty, animationY);
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            AnimateHandPoint(_handRightPoint, _innerRect.TopLeft);
        }

        private void TransformHand()
        {
            TranslateTransform transform = new TranslateTransform();
            Hand.RenderTransform = transform;
            transform.X = _handRightPoint.X - (Hand.Width / 2);
            transform.Y = _handRightPoint.Y - (Hand.Height / 2);
        }

        private void TransformRectangles()
        {
            TranslateTransform transformInnerRect = (TranslateTransform)InnerRect.RenderTransform;
            transformInnerRect.X = _innerRect.X;
            transformInnerRect.Y = _innerRect.Y;
            InnerRect.Width = _innerRect.Width;
            InnerRect.Height = _innerRect.Height;

            TranslateTransform transformOuterRect = (TranslateTransform)OuterRect.RenderTransform;
            transformOuterRect.X = _outerRect.X;
            transformOuterRect.Y = _outerRect.Y;
            OuterRect.Width = _outerRect.Width;
            OuterRect.Height = _outerRect.Height;
        }

        private void SwipeRight()
        {
        }

        private void SwipeLeft()
        {
        }

        private double GetPercentageSwipeLeft(Point hand)
        {

            double rectDistance = _innerRect.X - _outerRect.X;
            double handDistance = _innerRect.X - hand.X;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeRight(Point hand)
        {

            double rectDistance = (_outerRect.X + _outerRect.Width) - (_innerRect.X + _innerRect.Width);
            double handDistance = hand.X - (_innerRect.X + _innerRect.Width);
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeTop(Point hand)
        {

            double rectDistance = _innerRect.Y - _outerRect.Y;
            double handDistance = _innerRect.Y - hand.Y;
            return handDistance / rectDistance;
        }

        private double GetPercentageSwipeBottom(Point hand)
        {

            double rectDistance = (_outerRect.Y + _outerRect.Height) - (_innerRect.Y + _innerRect.Height);
            double handDistance = hand.Y - (_innerRect.Y + _innerRect.Height);
            return handDistance / rectDistance;
        }

        private void SwipeUp()
        {
        }

        private void SwipeDown()
        {
        }

        
        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
    }
}