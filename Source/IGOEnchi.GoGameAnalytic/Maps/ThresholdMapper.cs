using System;
using System.Collections.Generic;

namespace IGOPhoenix.GoGameAnalytic.Maps
{
    public class ThresholdMapper<TOut>
    {
        private readonly TOut defautValue;
        private readonly Func<double, double, bool> compareValueAndThreshold;
        private readonly List<ThresholdMap<TOut>> Thresholds = new List<ThresholdMap<TOut>>();

        public ThresholdMapper(TOut defautValue, Func<double,double,bool> compareValueAndThreshold)
        {
            this.defautValue = defautValue;
            this.compareValueAndThreshold = compareValueAndThreshold;
        }

        public ThresholdMapper<TOut> Add(double threshold, TOut value)
        {
            Thresholds.Add(new ThresholdMap<TOut>(threshold,value));
            return this;
        }

        public TOut Map(double input)
        {
            foreach (var mapping in Thresholds)
            {
                if (compareValueAndThreshold(input,mapping.Threshold)) return mapping.Value;
            }
            return defautValue;
        }

        class ThresholdMap<TOut>
        {
            public readonly double Threshold;
            public readonly TOut Value;

            public ThresholdMap(double threshold, TOut value)
            {

                this.Threshold = threshold;
                this.Value = value;
            }
        }
    }
}