using System;

namespace IGOEnchi.GoGameLogic
{
    public interface ICoords
    {
        byte X { get;  }
        byte Y { get;  }
    }
    

    public static class CoordsUtils
    {

        public static Double Distance(ICoords a, ICoords b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}