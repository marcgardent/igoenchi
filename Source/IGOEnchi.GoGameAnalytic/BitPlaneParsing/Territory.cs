using System.Collections.Generic;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class TerritoryParser
    {
        public enum BorderState
        {
            left,
            right,
            top,
            bottom
        }

        public class TerritoryCoords : ICoords
        {
            public IEnumerable<BorderState> Orientations { get; }
            public int X { get; }
            public int Y { get; }
            
            public TerritoryCoords(byte x, byte y)
            {
                X = x;
                Y = y;
            }

            public TerritoryCoords(ICoords coords, IEnumerable<BorderState> orientations)
            {
                Orientations = orientations;
                X = coords.X;
                Y = coords.Y;
            }
        }
        
        public List<TerritoryCoords> Parse(BitPlane bitPlane)
        {
            List<TerritoryCoords> ret  = new List<TerritoryCoords>();
            foreach (var coord in bitPlane.Unabled )
            {
                var limits = new List<BorderState>();
                
                var left = new Coords(coord.X - 1, coord.Y);
                var right = new Coords(coord.X + 1, coord.Y);
                var top = new Coords(coord.X , coord.Y-1);
                var bottom = new Coords(coord.X , coord.Y+1);
                /*
                if (!bitPlane.InOfRange(left) || !bitPlane[left]) limits.Add(BorderState.left); 
                if (!bitPlane.InOfRange(right) || !bitPlane[right]) limits.Add(BorderState.right); 
                if (!bitPlane.InOfRange(top) || !bitPlane[top]) limits.Add(BorderState.top); 
                if (!bitPlane.InOfRange(bottom) || !bitPlane[bottom]) limits.Add(BorderState.bottom);
                */

                if (bitPlane.InOfRange(left) &&!bitPlane[left]) limits.Add(BorderState.left);
                if (bitPlane.InOfRange(right) && !bitPlane[right]) limits.Add(BorderState.right);
                if (bitPlane.InOfRange(top) && !bitPlane[top]) limits.Add(BorderState.top);
                if (bitPlane.InOfRange(bottom) && !bitPlane[bottom]) limits.Add(BorderState.bottom);

                ret.Add(new TerritoryCoords(coord, limits));
            }

            return ret;
        }
    }
}