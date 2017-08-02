using System;
using System.Collections.Generic;
using System.Xml;

namespace IGOPhoenix.GoGameAnalytic.Maps
{
    public interface IMapper<TOut>
    {
        TOut Map(double input);
    }

    public class ThresholdMapper<TOut> : IMapper<TOut>
    {
        
        private readonly List<ThresholdMap<TOut>> Thresholds = new List<ThresholdMap<TOut>>();
         

        public ThresholdMapper<TOut> Gt(double threshold, TOut value)
        {
            Thresholds.Add(new ThresholdMap<TOut>(threshold,value, gt));
            return this;
        }

        public ThresholdMapper<TOut> Le(double threshold, TOut value)
        {
            Thresholds.Add(new ThresholdMap<TOut>(threshold, value, le));
            return this;
        }

        public ThresholdMapper<TOut> Ge(double threshold, TOut value)
        {
            Thresholds.Add(new ThresholdMap<TOut>(threshold, value, ge));
            return this;
        }

        public ThresholdMapper<TOut> Lt(double threshold, TOut value)
        {
            Thresholds.Add(new ThresholdMap<TOut>(threshold, value, lt));
            return this;
        }

        private static bool gt(double a, double b) => a > b;
        private static bool lt(double a, double b) => a < b;
        private static bool le(double a, double b) => a <= b;
        private static bool ge(double a, double b) => a >= b;

        public TOut Map(double input)
        {
            foreach (var mapping in Thresholds)
            {
                if (mapping.Resolve(input)) return mapping.Value;
            }
            throw new ArgumentOutOfRangeException("input", input, "Not in any range");
        }

        class ThresholdMap<TValue>
        {
            public readonly double Threshold;
            public readonly TValue Value;
            private readonly Func<double, double, bool> compare;

            public ThresholdMap(double threshold, TValue value, Func<double, double, bool> compare)
            {

                this.Threshold = threshold;
                this.Value = value;
                this.compare = compare;
            }

            public bool Resolve(double value)
            {
                return compare(value, Threshold);
            }
        }
    }
}