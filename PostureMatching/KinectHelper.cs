//------------------------------------------------------------------------------
// KinectHelper.cs
//------------------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Kinect.Toolkit;

    /// <summary>
    /// Static helper class for this module
    /// </summary>
    public static class KinectHelper
    {
        /// <summary>
        /// Get the depth image size from the input depth image format.
        /// </summary>
        public static Size GetDepthImageSize(DepthImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case DepthImageFormat.Resolution320x240Fps30:
                    return new Size(320, 240);

                case DepthImageFormat.Resolution640x480Fps30:
                    return new Size(640, 480);

                case DepthImageFormat.Resolution80x60Fps30:
                    return new Size(80, 60);
            }

            throw new ArgumentOutOfRangeException("imageFormat");
        }

        /// <summary>
        /// Get the color image size from the input color image format.
        /// </summary>
        public static Size GetColorImageSize(ColorImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ColorImageFormat.InfraredResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.RawBayerResolution1280x960Fps12:
                    return new Size(1280, 960);

                case ColorImageFormat.RawBayerResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.RawYuvResolution640x480Fps15:
                    return new Size(640, 480);

                case ColorImageFormat.RgbResolution1280x960Fps12:
                    return new Size(1280, 960);

                case ColorImageFormat.RgbResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.YuvResolution640x480Fps15:
                    return new Size(640, 480);
            }

            throw new ArgumentOutOfRangeException("imageFormat");
        }
    }
}
