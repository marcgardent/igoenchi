using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public static class AnalyticHelper
    {

        public delegate IEnumerable<ICoords> WalkHandler(BitPlane scope, ICoords target);
        
        public static IEnumerable<ICoords> NeighbourCoord(ICoords target)
        {
            foreach (var coordse in HortogonalCoord(target)) yield return coordse;
            foreach (var coordse in DiagonalCoord(target)) yield return coordse;
        }

        public static IEnumerable<ICoords> DiagonalCoord(ICoords target)
        {
            yield return new Coords((target.X + 1), (target.Y-1));
            yield return new Coords((target.X - 1), (target.Y + 1));
            yield return new Coords((target.X + 1), (target.Y + 1));
            yield return new Coords((target.X + 1), (target.Y - 1));
        }

        public static IEnumerable<ICoords> HortogonalCoord(ICoords target)
        {
            yield return new Coords((target.X + 1), target.Y);
            yield return new Coords((target.X - 1), target.Y);
            yield return new Coords(target.X, (target.Y + 1));
            yield return new Coords(target.X, (target.Y - 1));
        }
        public static IEnumerable<ICoords> StandardWalk(BitPlane scope, ICoords target)
        {
            return NeighbourCoord(target).Where(x => scope.InOfRange(x) && scope[x]);
        }

        public static IEnumerable<ICoords> NobiWalk(BitPlane scope, ICoords target)
        {
            return HortogonalCoord(target).Where(x => scope.InOfRange(x) && scope[x]);
        }


        public static IEnumerable<ICoords> Circle(ICoords start, int r)
        {
            var left = new Coords(start.X - r, start.Y);
            for (int i = 0; i < r; i++)
            {
                yield return left;
                left= new Coords(left.X+1,left.Y-1);
            }

            var top = new Coords(start.X, start.Y-r);
            for (int j = 0; j < r; j++)
            {
                yield return top;
                top = new Coords(top.X + 1, top.Y + 1);
            }

            var right = new Coords(start.X+r, start.Y);
            for (int k = 0; k < r; k++)
            {
                yield return right;
                    right = new Coords(right.X - 1, right.Y + 1);
            }

            var bottom = new Coords(start.X, start.Y + r);
            for (int l = 0; l < r; l++)
            {
                yield return bottom;
                bottom = new Coords(bottom.X - 1, bottom.Y - 1);
            }
        }

        public static IEnumerable<IEnumerable<ICoords>> OilStainWalker(BitPlane space, ICoords start)
        {
            IEnumerable<ICoords> ret;
            var i = 1;
            do
            {
                ret = Circle(start, i++).Where(space.InOfRange).ToList();
                yield return ret;
            } while (ret.Any());
        }

    }
}