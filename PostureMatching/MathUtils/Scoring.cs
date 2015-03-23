using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtils
{
    public static class Scoring
    {
        private const double LowThreshold = 0.2;

        private const double HighThreshold = 1.0;

        public static int GetScore(double distance)
        {
            if (distance <= LowThreshold)
            {
                return 100;
            }
            else if (distance >= HighThreshold)
            {
                return 0;
            }
            else
            {
                return (int)((HighThreshold - distance) * 100 / (HighThreshold - LowThreshold));
            }
        }
    }
}
