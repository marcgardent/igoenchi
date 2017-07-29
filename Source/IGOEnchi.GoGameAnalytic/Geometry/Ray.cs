using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Geometry
{
    public class RayTracer
    {
        private readonly double[,] output;

        public RayTracer(BitPlane input, BitPlane all)
        {
            this.output = new double[input.Width, input.Height];
            
            foreach (var coordse in input.Unabled)
            {
                   
            }
        }
        
        private RayTracer(ICoords coords)
        {
        
        }
    }

    public class Ray
    {
        private readonly ICoords origin;
        private readonly ICoords destination;


        public Ray(ICoords origin, ICoords destination)
        {
            this.origin = origin;
            this.destination = destination;
        }

        public IEnumerable<ICoords> Points { 
            get
            {
                var ret = origin;
                while (ret.X!=destination.X || ret.Y != destination.Y)
                {
                    ret = Next(ret, destination);
                    yield return ret;
                }
            }
        }


        public static ICoords Next(ICoords origin, ICoords destination)
        {
            var candidates = new List<Coords>();

            var top = destination.Y < origin.Y;
            var bottom = destination.Y > origin.Y;
            var left = destination.X < origin.X;
            var right = destination.X > origin.X;

            //Priority for hortogonal moves
            if (top) candidates.Add(new Coords(origin.X, (byte)(origin.Y - 1)));
            if (bottom) candidates.Add(new Coords(origin.X, (byte)(origin.Y + 1)));
            if (left) candidates.Add(new Coords((byte)(origin.X-1), origin.Y));
            if (right) candidates.Add(new Coords((byte)(origin.X+1), origin.Y));
            
            //Diagonal move allowed
            if (top && left) candidates.Add(new Coords((byte)(origin.X - 1), (byte)(origin.Y - 1)));
            if (top && right) candidates.Add(new Coords((byte)(origin.X + 1), (byte)(origin.Y - 1)));
            if (bottom && left) candidates.Add(new Coords((byte)(origin.X - 1), (byte)(origin.Y + 1)));
            if (bottom && right) candidates.Add(new Coords((byte)(origin.X + 1), (byte)(origin.Y + 1)));

            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            double dmin = double.MaxValue;
            var result = candidates.First();
            foreach (var candidate in candidates)
            {
                var d = CoordsUtils.Distance(destination, candidate);
                if (d < dmin)
                {
                    dmin = d;
                    result = candidate;
                }
            }

            return result;
        }
    }
}
