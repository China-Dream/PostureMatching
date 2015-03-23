//------------------------------------------------------------------------------
// FunctionSettings.cs
//------------------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using Microsoft.Kinect;

    /// <summary>
    /// Class representing function settings
    /// </summary>
    [Serializable]
    public class FunctionSettings
    {
        public FunctionSettings()
        {
            this.NearMode = true;
            this.Checking = false;
            this.PStatus = PlayingStatus.None;
            this.BoneJoint = JointType.HipCenter;
            this.Status = "-";
            this.Mirror = false;
            this.UseWeight = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether enable near mode
        /// </summary>
        public bool NearMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether checking
        /// </summary>
        public bool Checking { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which smooth is used
        /// </summary>
        public PlayingStatus PStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which smooth is used
        /// </summary>
        public JointType BoneJoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what status is
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether do the mirror
        /// </summary>
        public bool Mirror { get; set; }

        public bool UseWeight { get; set; }
    }
}
