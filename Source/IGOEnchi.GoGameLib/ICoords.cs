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


        /// <summary>
        /// the distance from the given point to the segment
        /// from https://www.developpez.net/forums/d537417/general-developpement/algorithme-mathematiques/mathematiques/distance-d-point-segment/
        /// </summary>
        /// <param name="ps">point of line</param>
        /// <param name="pe">second point of line</param>
        /// <param name="p">p the given point</param>
        /// <returns></returns>
        public static double distanceToLine(ICoords ps, ICoords pe, ICoords p)
        {
             

            int sx = pe.X - ps.X;
            int sy = pe.Y - ps.Y;

            int ux = p.X - ps.X;
            int uy = p.Y - ps.Y;

            int dp = sx * ux + sy * uy; 
            int sn2 = sx * sx + sy * sy; 

            double ah2 = dp * dp / (double)sn2;
            int un2 = ux * ux + uy * uy;
            return Math.Sqrt(un2 - ah2);
        } 

        public static Double Distance(ICoords a, ICoords b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static bool Equals(ICoords a, ICoords b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
    }
}