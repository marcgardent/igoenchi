using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing
{

    public class RayCoords : ICoords    
    {
        public byte X { get; }
        public byte Y { get; }

        public readonly List<ICoords> Cousins = new List<ICoords>();

        public IEnumerable<ICoords> All()
        {
            yield return this;
            foreach (var coordse in Cousins)
            {
                yield return coordse;
            }
        }

        public RayCoords(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public RayCoords(ICoords coords)
        {
            X = coords.X;
            Y = coords.Y;

        }
    }

    public class Ray
    {
        private readonly RayCoords origin;
        private  RayCoords current;
        private readonly ICoords destination;
        
        private readonly bool top;
        private readonly bool bottom;
        private readonly bool left;
        private readonly bool right;
        private readonly double slope;
        

        public Ray(ICoords origin, ICoords destination)
        {
            this.origin = new RayCoords(origin);
            this.destination = destination;
            
            this.top = destination.Y < origin.Y;
            this.bottom = destination.Y > origin.Y;
            this.left = destination.X < origin.X;
            this.right = destination.X > origin.X;

            double dy = destination.Y - this.origin.Y;
            double dx = destination.X - this.origin.X;
            this.slope = dx==0 ? double.NaN : dy/dx;
            

        }

        public IEnumerable<RayCoords> Points { 
            get
            {
                current = origin;
                while (current.X != destination.X || current.Y != destination.Y)
                {
                    current = Next();
                    yield return current;
                }
            }
        }
         
        public RayCoords Next()
        {
 

            Coords v = null;
            Coords h = null;
            if (top)  v = new Coords(current.X, (byte)(current.Y - 1));
            if (bottom) v = new Coords(current.X, (byte)(current.Y + 1));
            if (left) h= new Coords((byte)(current.X - 1), current.Y);
            if (right) h= new Coords((byte)(current.X + 1), current.Y);
            
            // hortogonal cases
            if (v == null && h != null) return new RayCoords(h);
            if (v != null && h == null) return new RayCoords(v);


            var hdist = CoordsUtils.distanceToLine(origin, destination, h);
            var vdist = CoordsUtils.distanceToLine(origin, destination, v);

            if (Math.Abs(hdist - vdist) < 0.0000001) //"Miai" => diagonal cases
            {
                RayCoords ret;
                if (top && left) ret = new RayCoords((byte) (current.X - 1), (byte) (current.Y - 1));
                else if (top && right) ret = new RayCoords((byte) (current.X + 1), (byte) (current.Y - 1));
                else if (bottom && left) ret = new RayCoords((byte) (current.X - 1), (byte) (current.Y + 1));
                else ret = new RayCoords((byte) (current.X + 1), (byte) (current.Y + 1)); // (bottom && right)

                ret.Cousins.Add(h);
                ret.Cousins.Add(v);
                return ret;
            }
            else
            {
                return hdist > vdist ? new RayCoords(v) : new RayCoords(h);
            }
        }
    }
}

