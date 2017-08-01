using System;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps.Influence.Basic
{
    public class InfluenceMapBuilder : IMapProcessor
    { 
        private readonly Map map; 

        public InfluenceMapBuilder(BitPlane bitplane)
        {
            this.map = new Map(bitplane.Width, bitplane.Height);

            for (int x = 0; x < bitplane.Width; x++)
            for (int y = 0; y < bitplane.Height; y++)
            {
                double influence = 0.0;
                foreach (var coordse in bitplane.Unabled)
                {
                    double d= Math.Abs(x - coordse.X) + Math.Abs(y - coordse.Y);
                    influence += d==0 ? double.PositiveInfinity :  1.0/d;
                }
                
                this.map[x, y] = influence;
            }
        }

        public Map GetMap()
        {
            return map;
        }
    }
}