using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MathUtils
{
    public class SkeletonStreamComparer
    {
        private const int PreCompareFrames = 1;  // Almost one frame

        private const int PostCompareFrames = 3; // Almost four frames

        // Save the data of ground truth
        private List<UnifiedSkeleton> groundTruthFrames = null;

        private List<double> result = new List<double>();

        // The file path of ground truth
        private string groundTruthPath;

        private int score = 0;

        public SkeletonStreamComparer(string groundTruthPath)
        {
            this.LoadGroundTruth(groundTruthPath);

            for (int i = 0; i < this.groundTruthFrames.Count; i++)
            {
                this.result.Add(UnifiedSkeleton.DistanceForInvalidBone);
            }
        }

        public SkeletonStreamComparer(List<UnifiedSkeleton> skelList, bool useWeight)
        {
            this.groundTruthFrames = skelList;

            for (int i = 0; i < this.groundTruthFrames.Count; i++)
            {
                this.result.Add(UnifiedSkeleton.DistanceForInvalidBone);
            }

            if (useWeight)
            {
                WeightGenerator.SetWeights(this.groundTruthFrames);
            }
            else
            {
                WeightGenerator.ResetWeights(this.groundTruthFrames);
            }
        }

        public int Score
        {
            get
            {
                return this.score;
            }
        }

        public void Start()
        {
            // Todo : MonoBehavior
        }

        public void Update(UnifiedSkeleton skel, int groundTruthFrameIndex)
        {
            // Update the result of the specified range
            int startFrameIndex = Math.Max(groundTruthFrameIndex - PostCompareFrames, 0);
            int endFrameIndex = Math.Min(groundTruthFrameIndex + PreCompareFrames, this.groundTruthFrames.Count - 1);
            for (int index = startFrameIndex; index <= endFrameIndex; index++)
            {
                // Compare the frame with the current new frame and update the result
                double comparedResult = UnifiedSkeleton.ComputeDistance(this.groundTruthFrames[index], skel);
                if (this.result[index] > comparedResult)
                {
                    this.score += (Scoring.GetScore(comparedResult) - Scoring.GetScore(this.result[index]));
                    this.result[index] = comparedResult;
                }
            }
        }

        private void LoadGroundTruth(string filePath)
        {
            this.groundTruthFrames.Clear();

            // Todo : fill the ground truth frame list
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace MathUtils
//{
//    public class SkeletonStreamComparer
//    {
//        private const int PreCompareInterval = 50;  // Almost one frame

//        private const int PostCompareInterval = 140; // Almost four frames

//        // Save the data of ground truth
//        // The key is the timsstamp(ms) of the frame and the timestamp starts from the first frame
//        private SortedList<double, UnifiedSkeleton> groundTruthFrames = new SortedList<double, UnifiedSkeleton>();

//        private List<double> result = new List<double>();

//        // Indicate whether the first frame
//        private bool isFirstFrame = true;

//        private Stopwatch sw = new Stopwatch();

//        private int elapsedMilliseconds = 0;

//        // The file path of ground truth
//        private string groundTruthPath;

//        private int score = 0;

//        public SkeletonStreamComparer(string groundTruthPath)
//        {
//            this.LoadGroundTruth(groundTruthPath);

//            for (int i = 0; i < this.groundTruthFrames.Count; i++)
//            {
//                this.result.Add(UnifiedSkeleton.DistanceForInvalidBone);
//            }
//        }

//        public SkeletonStreamComparer(List<UnifiedSkeleton> skelList, List<double> timestampList)
//        {
//            if (skelList.Count == timestampList.Count && skelList.Count != 0)
//            {
//                for (int i = 0; i < skelList.Count; i++)
//                {
//                    this.groundTruthFrames.Add(timestampList[i], skelList[i]);
//                }
//            }

//            for (int i = 0; i < this.groundTruthFrames.Count; i++)
//            {
//                this.result.Add(UnifiedSkeleton.DistanceForInvalidBone);
//            }
//        }

//        public int Score
//        {
//            get
//            {
//                return this.score;
//            }
//        }

//        public void Start()
//        {
//            // Todo : MonoBehavior
//        }

//        public void Update(UnifiedSkeleton skel)
//        {
//            //// Todo : MonoBehavior. get the skeleton data and verify whether it is new data
//            //bool hasNewSkeleton = false;
//            //UnifiedSkeleton newSkeleton = null;
//            //// ...
//            //if (!hasNewSkeleton)
//            //{
//            //    return;
//            //}

//            if (this.isFirstFrame)
//            {
//                this.sw.Restart();
//                this.isFirstFrame = false;
//                this.score = 0;
//                this.elapsedMilliseconds = 0;
//            }
//            else
//            {
//                this.sw.Stop();
//                this.elapsedMilliseconds = (int)this.sw.ElapsedMilliseconds;
//                this.sw.Start();
//            }

//            // Update the result of the specified range
//            int floorFrameIndex = this.GetFrameIndex(this.elapsedMilliseconds - PostCompareInterval);
//            int ceilingFrameIndex = this.GetFrameIndex(this.elapsedMilliseconds + PreCompareInterval, false);
//            for (int index = floorFrameIndex; index < ceilingFrameIndex; index++)
//            {
//                // Compare the frame with the current new frame and update the result
//                double comparedResult = UnifiedSkeleton.ComputeDistance(this.groundTruthFrames.Values[index], skel);
//                if (this.result[index] > comparedResult)
//                {
//                    this.score += (Scoring.GetScore(comparedResult) - Scoring.GetScore(this.result[index]));
//                    this.result[index] = comparedResult;
//                }
//            }
//        }

//        public void Reset()
//        {
//            this.isFirstFrame = true;
//        }

//        private void LoadGroundTruth(string filePath)
//        {
//            this.groundTruthFrames.Clear();

//            // Todo : fill the ground truth frame list
//        }

//        private int GetFrameIndex(int timepoint, bool isFloor = true)
//        {
//            int low = 0;
//            int high = this.groundTruthFrames.Count - 1;
//            while (low < high)
//            {
//                int mid = (low + high) / 2;
//                if (this.groundTruthFrames.Keys[mid] == timepoint)
//                {
//                    return mid;
//                }
//                else if (this.groundTruthFrames.Keys[mid] > timepoint)
//                {
//                    high = mid - 1;
//                }
//                else
//                {
//                    low = mid + 1;
//                }
//            }
//            return isFloor ? low : high;
//        }
//    }
//}
