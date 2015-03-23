// -----------------------------------------------------------------------
// Converters.cs
// -----------------------------------------------------------------------

namespace WpfViewers
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using Microsoft.Kinect;

    /// <summary>
    /// Static helper class for this module
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Coerce the requested elevation angle to a valid angle
        /// </summary>
        /// <param name="sender">The source KinectManager</param>
        /// <param name="baseValue">The baseValue to coerce</param>
        /// <returns>A valid elevation angle.</returns>
        public static object CoerceElevationAngle(DependencyObject sender, object baseValue)
        {
            var sensorWrapper = sender as KinectManager;

            if ((null == sensorWrapper) || !(baseValue is int))
            {
                return 0;
            }

            // Best guess default values for min/max angles
            int minVal = -27;
            int maxVal = 27;

            if (null != sensorWrapper.Sensor)
            {
                minVal = sensorWrapper.Sensor.MinElevationAngle;
                maxVal = sensorWrapper.Sensor.MaxElevationAngle;
            }

            if ((int)baseValue < minVal)
            {
                return minVal;
            }

            if ((int)baseValue > maxVal)
            {
                return maxVal;
            }

            return baseValue;
        }
    }
}
