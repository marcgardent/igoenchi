using System.Collections.Generic;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class Territory
    {
        public enum Orientation
        {
            left,
            right,
            top,
            bottom
        }

        public class Limit : ICoords
        {
            public IEnumerable<Orientation> Orientations { get; }
            public int X { get; }
            public int Y { get; }
            
            public Limit(byte x, byte y)
            {
                X = x;
                Y = y;
            }

            public Limit(ICoords coords, IEnumerable<Orientation> orientations)
            {
                Orientations = orientations;
                X = coords.X;
                Y = coords.Y;

            }
        }

        public Territory(BitPlane bitPlane)
        {

            List<Limit> ret  = new List<Limit>();
            foreach (var coord in bitPlane.Unabled )
            {
                var limits = new List<Orientation>();
                /*
                if (coord.X==0 || bitPlane[coord.X-1,coord.Y]) limits.Add(Orientation.left); 
                if (coord.X > bitPlane.Height || bitPlane[coord.X+1,coord.Y]) limits.Add(Orientation.right); 
                if (coord.Y > bitPlane.Height || bitPlane[coord.X,coord.Y+1]) limits.Add(Orientation.left); 
                if (coord.Y==0 || bitPlane[coord.X,coord.Y-1]) limits.Add(Orientation.left); 
                */
            }
        }
    }
}