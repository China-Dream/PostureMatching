// -----------------------------------------------------------------------
// KinectChangedEventArgs.cs
// -----------------------------------------------------------------------

namespace Kinect.Toolkit
{
    using System;
    using Microsoft.Kinect;

    /// <summary>
    /// Args for the KinectChanged event
    /// </summary>
    public class KinectChangedEventArgs : EventArgs
    {
        public KinectChangedEventArgs(KinectSensor oldSensor, KinectSensor newSensor)
        {
            this.OldSensor = oldSensor;
            this.NewSensor = newSensor;
        }

        public KinectSensor OldSensor { get; private set; }

        public KinectSensor NewSensor { get; private set; }
    }
}