using System;
using System.Collections.Generic;
using System.Xml;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps
{
    public class Territories
    {
        public enum TerritoryState 
        {
            Black,
            White,
            Constest,
        }



        public Territories(Map influenceWhiteMap, Map influenceBlackMap)
        {
            
        }
    }

    public class LayersMap<TGroup>
    {
        private readonly Map map;
        private readonly IMapper<TGroup> mapping;
         
        public LayersMap(Map map, IMapper<TGroup> mapping)
        {
            this.map = map;
            this.mapping = mapping;
        }

        public Dictionary<TGroup,BitPlane> Layers(TGroup group)
        {
            var ret = new Dictionary<TGroup, BitPlane>();

            foreach (var coords in map.AllCoords)
            {
                var grp = this.mapping.Map(map[coords]);

                if (!ret.ContainsKey(grp))
                {
                    ret[grp] = new BitPlane((byte)map.Width, (byte)map.Height);
                }
                ret[grp][coords] = true;
            }
            
            return ret;
        }
        
    }

    public interface IMapper<TOut>
    {
        TOut Map(double input);
    }

    public class ThresholdMapper<TOut> : IMapper<TOut>
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