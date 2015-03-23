//------------------------------------------------------------------------------
// KinectSettings.xaml.cs
//------------------------------------------------------------------------------

namespace WpfViewers
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for KinectSettings.xaml
    /// </summary>
    public partial class KinectSettings : UserControl
    {
        private KinectManager sensorManager = new KinectManager();

        #region Properties

        /// <summary>
        /// Property to KinectManager used for managing kinect sensor
        /// </summary>
        public KinectManager SensorManager
        {
            get
            {
                return this.sensorManager;
            }

            set
            {
                this.sensorManager = value;
            }
        }

        #endregion

        public KinectSettings()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.CaptureMouse())
                {
                    e.Handled = true;
                }
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.IsMouseCaptured)
                {
                    fe.ReleaseMouseCapture();
                    e.Handled = true;
                }
            }
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            var fe = sender as FrameworkElement;

            if (null != fe)
            {
                if (fe.IsMouseCaptured && (null != this.SensorManager) && (null != this.SensorManager.Sensor))
                {
                    var position = Mouse.GetPosition(this.SliderTrack);
                    int newAngle = -27 + (int)Math.Round(54.0 * (this.SliderTrack.ActualHeight - position.Y) / this.SliderTrack.ActualHeight);

                    if (newAngle < -27)
                    {
                        newAngle = -27;
                    }
                    else if (newAngle > 27)
                    {
                        newAngle = 27;
                    }

                    this.SensorManager.ElevationAngle = newAngle;
                }
            }
        }
    }
}
