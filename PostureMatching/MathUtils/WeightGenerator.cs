using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace MathUtils
{
    public static class WeightGenerator
    {
        private static int PreviewFrameCount = 2;

        private static int BufferIndex = 0;

        private static List<Dictionary<JointType, double>> MovementList = null;

        // Set the weights of the unified skeleton and here we use the acculated distance as weight
        public static void SetWeights(List<UnifiedSkeleton> skeletonList)
        {
            if (skeletonList == null)
            {
                return;
            }

            // Reset
            if (MovementList == null)
            {
                MovementList = new List<Dictionary<JointType, double>>();
                for (int i = 0; i < PreviewFrameCount; i++)
                {
                    MovementList.Add(null);
                }
            }
            

            for (int i = 1; i < skeletonList.Count(); i++)
            {
                Dictionary<JointType, double> weights = skeletonList[i].Weights;
                Dictionary<JointType, Point3D> preFramePositions = skeletonList[i-1].OriginalPositions;
                Dictionary<JointType, double> movement = new Dictionary<JointType, double>();

                foreach (var item in skeletonList[i].OriginalPositions)
                {
                    double distance = 0;
                    if (preFramePositions.ContainsKey(item.Key))
                    {
                        distance = (item.Value - preFramePositions[item.Key]).Length;
                    }
                    movement[item.Key] = distance;

                    if (i > PreviewFrameCount)
                    {
                        double acculatedDistance = distance;
                        for (int j = 0; j < PreviewFrameCount; j++)
                        {
                            if (MovementList[j].ContainsKey(item.Key))
                            {
                                acculatedDistance += MovementList[j][item.Key];
                            }
                        }
                        weights[item.Key] = acculatedDistance * 200.0 + UnifiedSkeleton.DefaultWeight;
                    }
                }

                MovementList[BufferIndex] = movement;
                BufferIndex = ++BufferIndex % PreviewFrameCount;
            }
        }
        
        public static void ResetWeights(List<UnifiedSkeleton> skeletonList)
        {
            for (int i = 0; i < skeletonList.Count; i++)
            {
                skeletonList[i].Weights.Clear();
            }
        }
    }
}
