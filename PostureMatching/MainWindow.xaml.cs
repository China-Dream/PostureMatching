namespace PostureMatching
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using Kinect.Toolkit;
    using WpfViewers;
    using MathUtils;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Fields

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Managing Kinect sensor
        /// </summary>
        private KinectSensorManager sensorManager = new KinectSensorManager();

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorFrameBitmap;

        /// <summary>
        /// Used for displaying the skeleton
        /// </summary>
        private KinectSkeleton kinectSkeleton;

        // 
        private KinectSkeleton playerSkeleton;

        //
        private int playingIndex = 0;

        private SkeletonStreamComparer ssc = null;

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        #region Properties

        /// <summary>
        /// Binding property to KinectSensor
        /// </summary>
        public KinectSensor Sensor
        {
            get
            {
                return this.sensorManager.Sensor;
            }
        }

        /// <summary>
        /// Binding property to KinectSensorChooser
        /// </summary>
        public KinectSensorChooser SensorChooser
        {
            get
            {
                return this.sensorManager.SensorChooser;
            }
        }

        #endregion

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Really dispose resources.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (this.sensorManager != null)
                {
                    this.sensorManager.Dispose();
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Binding sensor property
            var kinectSettingsBinding = new Binding("Sensor") { Source = this.sensorManager };
            BindingOperations.SetBinding(this.kinectSettings.SensorManager, KinectManager.SensorProperty, kinectSettingsBinding);

            var nearModeBinding = new Binding("NearMode") { Source = this.functionSettings, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(this.sensorManager, KinectSensorManager.NearModeProperty, nearModeBinding);

            var checkingBinding = new Binding("Checking") { Source = this.functionSettings, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(this.sensorManager, KinectSensorManager.CheckingProperty, checkingBinding);

            var mirrorBinding = new Binding("Mirror") { Source = this.functionSettings, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(this.sensorManager, KinectSensorManager.MirrorProperty, mirrorBinding);

            var pstatusBinding = new Binding("PStatus") { Source = this.functionSettings, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(this.sensorManager, KinectSensorManager.PStatusProperty, pstatusBinding);

            var useWeightBinding = new Binding("UseWeight") { Source = this.functionSettings, Mode = BindingMode.TwoWay };
            BindingOperations.SetBinding(this.sensorManager, KinectSensorManager.UseWeightProperty, useWeightBinding);

            // Register
            this.sensorManager.KinectDataEvent += this.OnKinectDataReady;

            // Start Kinect sensor chooser
            this.sensorChooserUI.KinectSensorChooser = this.SensorChooser;
            this.SensorChooser.KinectChanged += this.OnKinectSensorChanged;
            this.SensorChooser.Start();

            // Set Stand value as the default value
            this.statusBarText.Text = Properties.Resources.IntroductoryMessage;

            // Start the worker thread
            this.sensorManager.StartWorkerThread();
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unregister
            this.sensorManager.KinectDataEvent -= this.OnKinectDataReady;

            // Unregister Kinect sensor chooser event
            if (this.SensorChooser != null)
            {
                this.SensorChooser.KinectChanged -= this.OnKinectSensorChanged;
            }

            // Stop sensor
            if (this.sensorManager != null)
            {
                this.sensorManager.StopSensor();
            }

            // Stopt the worker thread
            this.sensorManager.StopWorkerThread();
        }

        /// <summary>
        /// Handler function for Kinect changed event
        /// </summary>
        /// <param name="sender">Event generator</param>
        /// <param name="e">Event parameter</param>
        private void OnKinectSensorChanged(object sender, KinectChangedEventArgs e)
        {
            // Call sensor manager to handle the related event
            this.sensorManager.OnKinectSensorChanged(sender, e);
        }

        /// <summary>
        /// Handler for rendering Kinect data
        /// </summary>
        private void OnKinectDataReady(object sender, EventArgs e)
        {
            if (this.sensorManager != null)
            {
                if (null == this.kinectSkeleton)
                {
                    this.kinectSkeleton = new KinectSkeleton(this.sensorManager.Sensor.CoordinateMapper);
                }
                if (this.sensorManager.PStatus == PlayingStatus.Playing)
                {
                    if (this.ssc == null)
                    {
                        this.ssc = new SkeletonStreamComparer(this.sensorManager.RecordingUnifiedSkeletons, this.sensorManager.UseWeight);
                    }

                    if (this.playingIndex < this.sensorManager.RecordingSkeletons.Count)
                    {
                        this.kinectSkeleton.DrawSkeleton(this.sensorManager.RecordingSkeletons[this.playingIndex], this.kinectImage);
                    }
                    this.playingIndex++;
                }
                else
                {
                    this.ssc = null;
                    this.playingIndex = 0;
                    RenderingHelper.WriteColorFrameToBitmap(
                        this.sensorManager.ImagePixels,
                        this.sensorManager.ColorWidth,
                        this.sensorManager.ColorHeight,
                        ref this.colorFrameBitmap);
                    this.kinectSkeleton.DrawSkeletonOnColorImage(
                        this.sensorManager.PStatus == PlayingStatus.Recording ? this.sensorManager.SelectedSkeleton : this.sensorManager.GroundTruchSkeleton,
                        this.colorFrameBitmap,
                        this.kinectImage);
                }

                // Render the player skeleton
                if (null == this.playerSkeleton)
                {
                    this.playerSkeleton = new KinectSkeleton(this.sensorManager.Sensor.CoordinateMapper);
                }
                this.playerSkeleton.DrawSkeleton(
                    this.sensorManager.SelectedSkeleton,
                    this.skeletonImage);

                //
                if (this.sensorManager.PStatus == PlayingStatus.Recording)
                {
                    this.functionSettings.Status = "Recording frames " + this.sensorManager.RecordingSkeletons.Count.ToString();
                }
                else if (this.sensorManager.PStatus == PlayingStatus.Playing)
                {
                    this.ssc.Update(new UnifiedSkeleton(this.sensorManager.SelectedSkeleton), this.playingIndex - 1);
                    
                    this.functionSettings.Status = "Score " + this.ssc.Score.ToString();
                }
                else if (this.sensorManager.Checking && this.sensorManager.GroundTruthUnifiedSkeleton != null
                    && this.sensorManager.GroundTruthUnifiedSkeleton.IsValid)
                {
                    JointType joint = this.functionSettings.BoneJoint;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(UnifiedSkeleton.ComputeDistance(this.sensorManager.GroundTruthUnifiedSkeleton, this.sensorManager.PlayerUnifiedSkeleton) + " ");
                    sb.Append(this.sensorManager.GroundTruthUnifiedSkeleton.Bones[joint].NormalizedBone + " ");
                    Vector3D vec = new Vector3D(0, 0, 0);
                    if (this.sensorManager.PlayerUnifiedSkeleton.IsValid)
                    {
                        sb.Append(this.sensorManager.PlayerUnifiedSkeleton.Bones[joint].NormalizedBone + " ");
                        vec = this.sensorManager.PlayerUnifiedSkeleton.Bones[joint].NormalizedBone;
                    }
                    else
                    {
                        sb.Append(vec + " ");
                    }
                    sb.Append(Vector3D.Distance(this.sensorManager.GroundTruthUnifiedSkeleton.Bones[joint].NormalizedBone, vec).ToString("0.000"));
             
                    this.functionSettings.Status = sb.ToString();
                }
            }
        }
    }
}
