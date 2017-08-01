using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps
{
    public interface IMapProcessor
    {
        Map GetMap();
    }


    public class Map
    {
        public readonly int Width;
        public readonly int Height;
        private readonly double[,] Data;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new double[width,height];
             
            this.Max = double.MinValue;
            this.Min = double.MaxValue;
        }

        public double Min { get; set; }
        public double Max { get; set; }

        public byte IntensityWithMinMax(ICoords coords)
        {
            var raw = this[coords];
            var normalized = double.IsPositiveInfinity(raw) ? 255 : ((raw - Min) / (Max-Min) * 255);
            return (byte)Math.Min(255, normalized);
        }

        public byte Intensity(ICoords coords)
        {
            var raw = this[coords];
            return (byte)Math.Min(255, raw/4d * 255);
        }
        
        public double this[ICoords coords]
        {
            get { return this[coords.X, coords.Y]; }

            set { this[coords.X, coords.Y] = value; }
        }

        public double this[int xIndex, int yIndex]
        {
            get { return Data[xIndex, yIndex]; }

            set
            {
                Data[xIndex, yIndex] = value;
                if (!double.IsInfinity(value)) { 
                    Min = Math.Min(Min, value);
                    Max = Math.Max(Max, value);
                }
            }
        }
        
        public bool InOfRange(ICoords coords)
        {
            return (coords.X < Width && coords.Y < Height);
        }
        
        public IEnumerable<ICoords> AllCoords
        {
            get
            {
                for (byte x = 0; x < this.Width; x++)
                {
                    for (byte y = 0; y < this.Height; y++)
                    {
                        yield return new Coords(x, y);
                    }
                }
            }
        }

        
    }
}
