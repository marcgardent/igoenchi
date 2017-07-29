using System;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class InfluenceMap
    { 
        private readonly double[,] map;
        public readonly double Max;
        public readonly double Min;
        public readonly double Amplitude;

        public InfluenceMap(BitPlane bitplane)
        {
            this.map = new double[bitplane.Width, bitplane.Height];
            Max = 0;
            Min = double.MaxValue;

            for (int x = 0; x < bitplane.Width; x++)
                for (int y = 0; y < bitplane.Height; y++)
                {
                    double influence = 0.0;
                    foreach (var coordse in bitplane.Unabled)
                    {
                        double d= Math.Abs(x - coordse.X) + Math.Abs(y - coordse.Y);
                        influence += d==0 ? double.PositiveInfinity :  1.0/d;
                    }

                    if (!double.IsPositiveInfinity(influence))
                    {
                        Max = Math.Max(Max, influence);
                        Min= Math.Min(Min, influence);
                    }
                    this.map[x, y] = influence;
                }
            Amplitude =  Max - Min;
        }

        public byte GetAbsolute(int x, int y)
        {
            var raw = this.map[x, y];
            return (byte)Math.Min(255, raw * 255);
        }

        public byte GetByte(int x, int y)
        {
            var raw = this.map[x, y];
            var normalized = double.IsPositiveInfinity(raw) ? 255 : ((raw-Min)/Amplitude * 255);
            return (byte)Math.Min(255, normalized);
        }
    }
}