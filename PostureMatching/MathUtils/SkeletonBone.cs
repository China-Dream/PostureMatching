using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace MathUtils
{
    public class SkeletonBone
    {
        // The end joint of the bone used as the bone type
        private JointType boneType;

        // Indicate if the joints of the bone are tracked
        private bool isValid;

        // The bone normalized vector in unified coordinate system
        private Vector3D normalizedBone;

        public SkeletonBone(Point3D start, Point3D end, JointType type, bool isValid)
        {
            this.boneType = type;
            this.isValid = isValid;
            this.normalizedBone = end - start;
            if (this.normalizedBone != Vector3D.Zero)
            {
                this.normalizedBone.Normalize();
            }
            else
            {
                this.isValid = false;
            }
        }

        public JointType BoneType
        {
            get
            {
                return this.boneType;
            }
        }
        
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public Vector3D NormalizedBone
        {
            get
            {
                return this.normalizedBone;
            }
        }
    }
}
