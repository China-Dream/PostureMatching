//------------------------------------------------------------------------------
// KinectSensorManager.cs
//------------------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Microsoft.Kinect;
    using Kinect.Toolkit;
    using MathUtils;

    /// <summary>
    /// Class manager the kinect sensor
    /// </summary>
    public class KinectSensorManager : DependencyObject, IDisposable, INotifyPropertyChanged
    {
        #region Constants

        // Color image format
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        #endregion

        #region Fields

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        // Indicate whether enable near mode
        public static readonly DependencyProperty NearModeProperty = DependencyProperty.Register(
            "NearMode",
            typeof(bool),
            typeof(KinectSensorManager),
            new PropertyMetadata(true, (o, args) => ((KinectSensorManager)o).UpdateNearMode()));

        // Indicate whether execute recording
        public static readonly DependencyProperty CheckingProperty = DependencyProperty.Register(
            "Checking",
            typeof(bool),
            typeof(KinectSensorManager),
            new PropertyMetadata(false, (o, args) => ((KinectSensorManager)o).UpdateChecking()));

        // Indicate whether do the mirror action
        public static readonly DependencyProperty MirrorProperty = DependencyProperty.Register(
            "Mirror",
            typeof(bool),
            typeof(KinectSensorManager),
            new PropertyMetadata(false));

        // Playing status 
        public static readonly DependencyProperty PStatusProperty = DependencyProperty.Register(
            "PStatus",
            typeof(PlayingStatus),
            typeof(KinectSensorManager),
            new PropertyMetadata(PlayingStatus.None, (o, args) => ((KinectSensorManager)o).UpdatePlaying()));

        //
        public static readonly DependencyProperty UseWeightProperty = DependencyProperty.Register(
            "UseWeight",
            typeof(bool),
            typeof(KinectSensorManager),
            new PropertyMetadata(false));

        // Kinect sensor chooser used for managing sensor hardware
        private KinectSensorChooser sensorChooser = new KinectSensorChooser();

        // The active sensor
        private KinectSensor sensor;

        /// <summary>
        /// The worker thread to recognize posture
        /// </summary>
        private Thread workerThread = null;

        /// <summary>
        /// Event to stop worker thread
        /// </summary>
        private ManualResetEvent workerThreadStopEvent;

        /// <summary>
        /// Event to notify that data is ready for processing
        /// </summary>
        private ManualResetEvent dataReadyEvent;

        /// <summary>
        /// Image width of color frame
        /// </summary>
        private int colorWidth = 0;

        /// <summary>
        /// Image height of color frame
        /// </summary>
        private int colorHeight = 0;

        /// <summary>
        /// Intermediate storage for the color data received from the camera in 32bit color
        /// </summary>
        private byte[] imagePixels;

        /// <summary>
        /// The selected skeleton data
        /// </summary>
        private Skeleton selectedSkeleton;

        // Selected skeleton tracking id
        private int skeletonTrackingId = 0;

        // The skeleton used for ground truth
        private Skeleton groundTruthSkeleton;

        // Delegate for Event for required on rendering Kinect data
        internal delegate void KinectDataEventHandler(object sender, EventArgs e);

        // Event for required on Kinect data ready
        internal event KinectDataEventHandler KinectDataEvent;

        private UnifiedSkeleton groundTruthUnifiedSkeleton = null;

        private UnifiedSkeleton playerUnifiedSkeleton = null;

        // Recording skeleton list used as rendering source
        private List<Skeleton> recordingSkeletons = new List<Skeleton>(); 

        // Recoridng unified skeleton list used to compare skeletons
        private List<UnifiedSkeleton> recordingUnifiedSkeletons = new List<UnifiedSkeleton>();

        // Recording skeleton frame timestamp list used to generate elepased
        private List<DateTime> recordingSkeletonTimestamps = new List<DateTime>();

        // Recording skeleton frame timestamp list used to compare skeletons
        private List<double> recordingSkeletonElapsed = new List<double>();

        #endregion


        public KinectSensorManager()
        {
        }

        #region Properties

        // Property changed event
        public event PropertyChangedEventHandler PropertyChanged;

        public KinectSensorChooser SensorChooser
        {
            get
            {
                return this.sensorChooser;
            }
        }

        // Binding property to Kinect Sensor
        public KinectSensor Sensor
        {
            get
            {
                return this.sensor;
            }

            set
            {
                this.sensor = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Sensor"));
                }
            }
        }

        public bool NearMode
        {
            get
            {
                return (bool)GetValue(NearModeProperty);
            }

            set
            {
                SetValue(NearModeProperty, value);
            }
        }

        public bool Checking
        {
            get
            {
                return (bool)GetValue(CheckingProperty);
            }

            set
            {
                SetValue(CheckingProperty, value);
            }
        }

        public bool Mirror
        {
            get
            {
                return (bool)GetValue(MirrorProperty);
            }

            set
            {
                SetValue(MirrorProperty, value);
            }
        }

        public bool UseWeight
        {
            get
            {
                return (bool)GetValue(UseWeightProperty);
            }

            set
            {
                SetValue(UseWeightProperty, value);
            }
        }

        public PlayingStatus PStatus
        {
            get
            {
                return (PlayingStatus)GetValue(PStatusProperty);
            }

            set
            {
                SetValue(PStatusProperty, value);
            }
        }

        public byte[] ImagePixels
        {
            get
            {
                return this.imagePixels;
            }
        }

        public Skeleton SelectedSkeleton
        {
            get
            {
                return this.selectedSkeleton;
            }
        }

        public Skeleton GroundTruchSkeleton
        {
            get
            {
                return this.groundTruthSkeleton;
            }
        }

        public int ColorWidth
        {
            get
            {
                return this.colorWidth;
            }
        }

        public int ColorHeight
        {
            get
            {
                return this.colorHeight;
            }
        }

        public UnifiedSkeleton GroundTruthUnifiedSkeleton
        {
            get
            {
                return this.groundTruthUnifiedSkeleton;
            }
        }

        public UnifiedSkeleton PlayerUnifiedSkeleton
        {
            get
            {
                return this.playerUnifiedSkeleton;
            }
        }

        public List<Skeleton> RecordingSkeletons
        {
            get
            {
                return this.recordingSkeletons;
            }
        }

        public List<UnifiedSkeleton> RecordingUnifiedSkeletons
        {
            get
            {
                return this.recordingUnifiedSkeletons;
            }
        }

        public List<double> RecordingSkeletonElapsed
        {
            get
            {
                return this.recordingSkeletonElapsed;
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
                if (this.workerThreadStopEvent != null)
                {
                    this.workerThreadStopEvent.Dispose();
                }

                if (this.dataReadyEvent != null)
                {
                    this.dataReadyEvent.Dispose();
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Start the worker thread
        /// </summary>
        public void StartWorkerThread()
        {
            if (null == this.workerThread)
            {
                this.workerThreadStopEvent = new ManualResetEvent(false);

                this.dataReadyEvent = new ManualResetEvent(false);
                
                // Create worker thread and start
                this.workerThread = new Thread(this.WorkerThreadProc);
                this.workerThread.Start();
            }
        }

        /// <summary>
        /// Stop the worker thread
        /// </summary>
        public void StopWorkerThread()
        {
            if (this.workerThread != null && this.workerThreadStopEvent != null)
            {
                this.workerThreadStopEvent.Set();

                this.workerThread.Join();
            }
        }

        /// <summary>
        /// Handler function for Kinect changed event
        /// </summary>
        /// <param name="sender">Event generator</param>
        /// <param name="e">Event parameter</param>
        public void OnKinectSensorChanged(object sender, KinectChangedEventArgs e)
        {
            // Check new sensor's status
            if (this.Sensor != e.NewSensor)
            {
                // Stop old sensor
                if (null != this.Sensor)
                {
                    this.StopSensor();
                }

                this.Sensor = null;

                if (e.NewSensor != null && KinectStatus.Connected == e.NewSensor.Status)
                {
                    this.Sensor = e.NewSensor;

                    // Start new sensor
                    this.StartStreams();
                }
            }
        }

        /// <summary>
        /// Stop sensor function
        /// </summary>
        public void StopSensor()
        {
            if (this.Sensor != null)
            {
                this.StopStreams();

                this.Sensor.Stop();
            }
        }

        /// <summary>
        /// Start sensor streams
        /// </summary>
        private void StartStreams()
        {
            try
            {
                Size colorImageSize = KinectHelper.GetColorImageSize(ColorFormat);
                this.colorWidth = (int)colorImageSize.Width;
                this.colorHeight = (int)colorImageSize.Height;

                // Enable streams, register event handler and start
                this.Sensor.ColorStream.Enable(ColorFormat);
                this.Sensor.SkeletonStream.Enable();

                this.Sensor.AllFramesReady += this.OnAllFramesReady;

                // Allocate frames
                this.AllocateFrames();

                this.Sensor.Start();
            }
            catch (IOException)
            {
                // Device is in use
                this.Sensor = null;

                return;
            }
            catch (InvalidOperationException)
            {
                // Device is not valid, not supported or hardware feature unavailable
                this.Sensor = null;

                return;
            }

            if (this.NearMode)
            {
                try
                {
                    this.Sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    this.NearMode = true;
                }
                catch (InvalidOperationException)
                {
                    // Near mode not supported on device, silently fail during initialization
                    this.NearMode = false;
                }
            }
        }

        /// <summary>
        /// Stop sensor streams
        /// </summary>
        private void StopStreams()
        {
            if (this.Sensor != null)
            {
                try
                {
                    this.Sensor.AllFramesReady -= this.OnAllFramesReady;

                    this.Sensor.ColorStream.Disable();
                    this.Sensor.SkeletonStream.Disable();

                    this.Sensor.Stop();
                }
                catch (InvalidOperationException)
                {
                    // Sliently ignore
                }
            }
        }

        /// <summary>
        /// Worker thread in which recognizing posture
        /// </summary>
        private void WorkerThreadProc()
        {
            ManualResetEvent[] waitEvents = new ManualResetEvent[2] { this.workerThreadStopEvent, this.dataReadyEvent };

            while (true)
            {
                int retIndex = ManualResetEvent.WaitAny(waitEvents);

                if (0 == retIndex)
                {
                    // Stop event has been set. Exit thread
                    break;
                }
                else if (1 == retIndex)
                {
                    this.dataReadyEvent.Reset();
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's AllFramesReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            // In the middle of shutting down etc, nothing to do
            if (null == this.Sensor)
            {
                return;
            }

            OnColorFrameReady(sender, allFramesReadyEventArgs);
            OnSkeletonFrameReady(sender, allFramesReadyEventArgs);

            if (this.KinectDataEvent != null)
            {
                this.KinectDataEvent(this, null);
            }
        }

        /// <summary>
        /// Allocate the frame buffers used in the process
        /// </summary>
        private void AllocateFrames()
        {
            int colorImageSize = this.colorWidth * this.colorHeight * sizeof(int);

            // Create local color pixels buffer
            this.imagePixels = new byte[colorImageSize];
        }

        // Handler for Kinect sensor's color data
        private void OnColorFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (this.imagePixels.Length != colorFrame.PixelDataLength)
                    {
                        this.colorWidth = colorFrame.Width;
                        this.colorHeight = colorFrame.Height;

                        this.AllocateFrames();
                    }

                    // Copy data to local buffer
                    if (colorFrame.PixelDataLength == this.imagePixels.Length && (!this.Checking || this.PStatus == PlayingStatus.Recording))
                    {
                        colorFrame.CopyPixelDataTo(this.imagePixels);
                    }
                }
            }
        }

        // Handler for Kinect sensor's skeleton data
        private void OnSkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // Reset the selected 
                this.selectedSkeleton = null;
                this.skeletonTrackingId = 0;

                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    // Copy data to local buffer
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    // Select proper skeleton
                    if (this.skeletonTrackingId != 0)
                    {
                        this.selectedSkeleton = (from s in skeletonData where s != null && s.TrackingId == this.skeletonTrackingId && SkeletonTrackingState.Tracked == s.TrackingState select s)
                            .FirstOrDefault();
                    }
                    else
                    {
                        this.selectedSkeleton = (from s in skeletonData where s != null && SkeletonTrackingState.Tracked == s.TrackingState select s)
                            .FirstOrDefault();
                    }

                    if (null != this.selectedSkeleton)
                    {
                        this.skeletonTrackingId = this.selectedSkeleton.TrackingId;
                    }

                    if (!this.Checking || this.groundTruthSkeleton == null)
                    {
                        this.groundTruthSkeleton = this.selectedSkeleton;
                    }
                    else
                    {
                        this.CompareSkeleton();
                    }

                    if (this.PStatus == PlayingStatus.Recording)
                    {
                        this.recordingSkeletons.Add(this.selectedSkeleton);
                        this.recordingUnifiedSkeletons.Add(new UnifiedSkeleton(this.selectedSkeleton));
                        this.recordingSkeletonTimestamps.Add(DateTime.Now);
                    }
                }
            }
        }

        // 
        private void UpdateChecking()
        {
            if (!this.Checking)
            {
                this.groundTruthUnifiedSkeleton = null;
            }
        }

        // Update near mode
        private void UpdateNearMode()
        {
            if (this.Sensor != null && this.NearMode != (this.Sensor.DepthStream.Range != DepthRange.Default))
            {
                try
                {
                    this.Sensor.SkeletonStream.EnableTrackingInNearRange = this.NearMode;
                }
                catch (InvalidOperationException)
                {
                    // Sliently fail
                    this.NearMode = false;
                }
            }
        }

        // Update the smooth parameters
        private void UpdatePlaying()
        {
            if (this.PStatus == PlayingStatus.Recording)
            {
                this.recordingSkeletons.Clear();
                this.recordingUnifiedSkeletons.Clear();
                this.recordingSkeletonTimestamps.Clear();
            }
            else if (this.PStatus == PlayingStatus.Playing)
            {
                this.recordingSkeletonElapsed.Clear();
                if (this.recordingSkeletonTimestamps.Count > 0)
                {
                    for (int i = 0; i < this.recordingSkeletonTimestamps.Count; i++)
                    {
                        this.recordingSkeletonElapsed.Add((this.recordingSkeletonTimestamps[i] - this.recordingSkeletonTimestamps[0]).TotalMilliseconds);
                    }
                }
            }
        }

        private void CompareSkeleton()
        {
            if (this.groundTruthSkeleton != null && this.groundTruthUnifiedSkeleton == null)
            {
                    this.groundTruthUnifiedSkeleton = new UnifiedSkeleton(this.groundTruthSkeleton);
            }

            this.playerUnifiedSkeleton = new UnifiedSkeleton(this.selectedSkeleton);
        }
    }
}
