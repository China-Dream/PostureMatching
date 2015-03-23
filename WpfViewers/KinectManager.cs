// -----------------------------------------------------------------------
// KinectManager.cs
// -----------------------------------------------------------------------

namespace WpfViewers
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.Kinect;

    /// <summary>
    /// Used for managing Kinect sensor
    /// Inherit DependencyObject for employing the dependency property
    /// </summary>
    public class KinectManager : DependencyObject
    {
        #region Fileds

        // Register the dependency property
        public static readonly DependencyProperty SensorProperty = DependencyProperty.Register(
            "Sensor",
            typeof(KinectSensor),
            typeof(KinectManager),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ElevationAngleProperty = DependencyProperty.Register(
            "ElevationAngle",
            typeof(int),
            typeof(KinectManager),
            new PropertyMetadata(0, (sender, args) => ((KinectManager)sender).EnsureElevationAngle(), Helper.CoerceElevationAngle));

        // The target value fo elevation angle
        private int targetElevationAngle = int.MinValue;

        // Whether finish the task of adjusting elevation angle
        private bool isElevationTaskOutstanding;

        #endregion

        public KinectManager()
        {
        }

        #region Properties

        // Bind property to KinectProperty
        public KinectSensor Sensor
        {
            get
            {
                return (KinectSensor)this.GetValue(SensorProperty);
            }

            set
            {
                this.isElevationTaskOutstanding = false;

                this.SetValue(SensorProperty, value);
            }
        }

        // Bind property to ElevationAngleProperty
        public int ElevationAngle
        {
            get
            {
                return (int)this.GetValue(KinectManager.ElevationAngleProperty);
            }

            set
            {
                this.SetValue(KinectManager.ElevationAngleProperty, value);
            }
        }

        #endregion

        public void Initialize()
        {
            this.EnsureElevationAngle();
        }

        /// <summary>
        /// Make sure adjusting the elevation angle to the set value
        /// </summary>
        private void EnsureElevationAngle()
        {
            var sensor = this.Sensor;
            if (null == sensor)
            {
                return;
            }

            // We cannot set the angle on a sensor if it is not running.
            // We will therefore call EnsureElevationAngle when the requested angle has changed or if the
            // sensor transitions to the Running state.
            if ((null == sensor) || (KinectStatus.Connected != sensor.Status) || !sensor.IsRunning)
            {
                return;
            }

            this.targetElevationAngle = this.ElevationAngle;

            // If there already a background task, it will notice the new targetElevationAngle
            if (!this.isElevationTaskOutstanding)
            {
                // Otherwise, we need to start a new task
                this.StartElevationTask();
            }
        }

        /// <summary>
        /// Function for adjusting the elevation angle
        /// </summary>
        private void StartElevationTask()
        {
            var sensor = this.Sensor;
            int lastSetElevationAngle = int.MinValue;

            if (null != sensor)
            {
                this.isElevationTaskOutstanding = true;

                Task.Factory.StartNew(
                    () =>
                    {
                        int angleToSet = this.targetElevationAngle;

                        // Keep going until we "match", assuming that the sensor is running
                        while ((lastSetElevationAngle != angleToSet) && sensor.IsRunning)
                        {
                            // We must wait at least 1 second, and call no more frequently than 15 times every 20 seconds
                            // So, we wait at least 1350ms afterwards before we set backgroundUpdateInProgress to false.
                            sensor.ElevationAngle = angleToSet;
                            lastSetElevationAngle = angleToSet;
                            Thread.Sleep(1350);

                            angleToSet = this.targetElevationAngle;
                        }
                    }).ContinueWith(
                            results =>
                            {
                                // This can happen if the Kinect transitions from Running to not running
                                // after the check above but before setting the ElevationAngle.
                                if (results.IsFaulted)
                                {
                                    var exception = results.Exception;

                                    Debug.WriteLine(
                                        "Set Elevation Task failed with exception " +
                                        exception);
                                }

                                // We caught up and handled all outstanding angle requests.
                                // However, more may come in after we've stopped checking, so
                                // we post this work item back to the main thread to determine
                                // whether we need to start the task up again.
                                this.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    if (this.targetElevationAngle !=
                                        lastSetElevationAngle)
                                    {
                                        this.StartElevationTask();
                                    }
                                    else
                                    {
                                        // If there's nothing to do, we can set this to false.
                                        this.isElevationTaskOutstanding = false;
                                    }
                                }));
                            });
            }
        }
    }
}
