﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GenderDetectorTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            detector.Start(KinectHelper.Instance.Sensor);
            KinectHelper.Instance.ReadyEvent += Instance_ReadyEvent;
        }

        void Instance_ReadyEvent(object sender, EventArgs e)
        {
            detector.SensorColorFrameReady(KinectHelper.Instance.GetFixedSkeleton(), KinectHelper.Instance.ColorPixels);
        }
    }
}
