//------------------------------------------------------------------------------
// RenderingHelper.cs
//------------------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    public static class RenderingHelper
    {
        // Minimu depth data
        public const int MinimumDepth = 0;

        // Maximum depth data
        public const int MaximumDepth = 6000;

        /// <summary>
        /// Render color frame with skeleton to UI direct from byte buffer
        /// </summary>
        public static void WriteColorFrameToBitmap(
            byte[] colorPixels, 
            int width, 
            int height, 
            ref WriteableBitmap bitmap)
        {
            if (null == colorPixels)
            {
                return;
            }

            if (null == bitmap || width != bitmap.Width || height != bitmap.Height)
            {
                // Create bitmap of correct format
                bitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr32, null);
            }

            // Write pixels to bitmap
            bitmap.WritePixels(
                        new Int32Rect(0, 0, width, height),
                        colorPixels,
                        bitmap.PixelWidth * sizeof(byte) * 4,   // rgba
                        0);
        }
    }
}
