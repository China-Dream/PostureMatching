using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace MathUtils
{
    public class UnifiedSkeleton
    {
        // Default weight if not set
        public const double DefaultWeight = 1.0;

        // Default distance for invalid bone vector, we set the maximum distance between two normalized vector
        public const double DistanceForInvalidBone = 2.0;

        // Save the original position of joints
        private Dictionary<JointType, Point3D> originalPositions = new Dictionary<JointType, Point3D>();
 
        // Save the unified position of joints
        private Dictionary<JointType, Point3D> positions = new Dictionary<JointType, Point3D>();

        // The key is the end joint of the bone, and the value is the bone vector
        private Dictionary<JointType, SkeletonBone> bones = new Dictionary<JointType, SkeletonBone>();

        // Indicate whether the data is valid
        private bool isValid = false;

        // Indicate the weights of bones
        private Dictionary<JointType, double> weights = new Dictionary<JointType, double>();
 
        public UnifiedSkeleton(Skeleton skel)
        {
            if (skel != null)
            {
                this.Initialize(skel);

                this.UnifyJoints();

                this.GenerateBones(skel);
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public Dictionary<JointType, Point3D> OriginalPositions
        {
            get
            {
                return this.originalPositions;
            }
        }

        public Dictionary<JointType, SkeletonBone> Bones
        {
            get
            {
                return this.bones;
            }
        }

        public Dictionary<JointType, double> Weights
        {
            get
            {
                return this.weights;
            }
        }

        public static double ComputeDistance(UnifiedSkeleton groundTruth, UnifiedSkeleton skel)
        {
            if (groundTruth == null || skel == null || !groundTruth.IsValid || groundTruth.Bones.Count == 0 || !skel.IsValid)
            {
                return DistanceForInvalidBone;
            }

            double discrepancy = 0.0;
            double normalizedDenominator = 0.0;
            foreach (var item in groundTruth.bones)
            {
                if (!item.Value.IsValid)
                {
                    // throw new Exception("It shouldn't have invalid data in ground truth skeleton!");
                    continue;
                }

                double weight = DefaultWeight;
                if (groundTruth.Weights.ContainsKey(item.Key))
                {
                    weight = groundTruth.Weights[item.Key];
                }
                normalizedDenominator += weight;

                double distance = DistanceForInvalidBone;
                if (skel.Bones.ContainsKey(item.Key) && skel.Bones[item.Key].IsValid)
                {
                    distance = Vector3D.Distance(item.Value.NormalizedBone, skel.Bones[item.Key].NormalizedBone);
                }
                discrepancy += distance * weight;
            }

            // Normalize the discrepancy to [0, 2]
            return normalizedDenominator != 0.0 ? discrepancy / normalizedDenominator : DistanceForInvalidBone;
        }

        private void Initialize(Skeleton skel)
        {
            if (skel != null && skel.TrackingState == SkeletonTrackingState.Tracked 
                && skel.Joints[JointType.HipCenter].TrackingState == JointTrackingState.Tracked
                && skel.Joints[JointType.HipLeft].TrackingState == JointTrackingState.Tracked
                && skel.Joints[JointType.HipRight].TrackingState == JointTrackingState.Tracked
                && skel.Joints[JointType.Spine].TrackingState == JointTrackingState.Tracked)
            {
                this.isValid = true;
            }
            else
            {
                return;
            }

            foreach (Joint joint in skel.Joints)
            {
                originalPositions.Add(joint.JointType, new Point3D(joint.Position.X, joint.Position.Y, joint.Position.Z));
            }
        }

        private void UnifyJoints()
        {
            if (this.isValid)
            {
                // Here we select HipCenter to Spine as Y, HipLeft to HipRight as X
                // And the create the coordinate system
                Vector3D vy = originalPositions[JointType.Spine] - originalPositions[JointType.HipCenter];
                Vector3D vx = originalPositions[JointType.HipRight] - originalPositions[JointType.HipLeft];
                vy.Normalize();
                vx.Normalize();
                Vector3D vz = Vector3D.CrossProduct(vx, vy);
                vz.Normalize();
                vx = Vector3D.CrossProduct(vy, vz);
                vx.Normalize();

                foreach (var item in this.originalPositions)
                {
                    Point3D unifiedPoint = new Point3D(
                        Vector3D.Project(item.Value, vx),
                        Vector3D.Project(item.Value, vy),
                        Vector3D.Project(item.Value, vz));
                    positions.Add(item.Key, unifiedPoint);
                }
            }
        }

        private void GenerateBones(Skeleton skel)
        {
            if (this.isValid)
            {
                this.CreateBone(JointType.HipCenter, JointType.Spine, skel);
                this.CreateBone(JointType.Spine, JointType.ShoulderCenter, skel);
                this.CreateBone(JointType.ShoulderCenter, JointType.ShoulderLeft, skel);
                this.CreateBone(JointType.ShoulderLeft, JointType.ElbowLeft, skel);
                this.CreateBone(JointType.ElbowLeft, JointType.WristLeft, skel);
                this.CreateBone(JointType.WristLeft, JointType.HandLeft, skel);
                this.CreateBone(JointType.ShoulderCenter, JointType.ShoulderRight, skel);
                this.CreateBone(JointType.ShoulderRight, JointType.ElbowRight, skel);
                this.CreateBone(JointType.ElbowRight, JointType.WristRight, skel);
                this.CreateBone(JointType.WristRight, JointType.HandRight, skel);
                this.CreateBone(JointType.ShoulderCenter, JointType.Head, skel);
                this.CreateBone(JointType.HipCenter, JointType.HipLeft, skel);
                this.CreateBone(JointType.HipLeft, JointType.KneeLeft, skel);
                this.CreateBone(JointType.KneeLeft, JointType.AnkleLeft, skel);
                this.CreateBone(JointType.AnkleLeft, JointType.FootLeft, skel);
                this.CreateBone(JointType.HipCenter, JointType.HipRight, skel);
                this.CreateBone(JointType.HipRight, JointType.KneeRight, skel);
                this.CreateBone(JointType.KneeRight, JointType.AnkleRight, skel);
                this.CreateBone(JointType.AnkleRight, JointType.FootRight, skel);
            }
        }

        private void CreateBone(JointType start, JointType end, Skeleton skel)
        {
            bool isValid = (skel.Joints[start].TrackingState == JointTrackingState.Tracked) 
                && (skel.Joints[end].TrackingState == JointTrackingState.Tracked);
            this.bones.Add(end, new SkeletonBone(this.positions[start], this.positions[end], end, isValid));
        }
    }
}
