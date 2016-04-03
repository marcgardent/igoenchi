using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public static class AnalyticHelper
    {
        public delegate IEnumerable<ICoords> WalkHandler(BitPlane scope, ICoords target);
        
        public static IEnumerable<ICoords> HortogonalCoord(ICoords target)
        {
            yield return new Coords((byte)(target.X + 1), target.Y);
            yield return new Coords((byte)(target.X - 1), target.Y);
            yield return new Coords(target.X, (byte)(target.Y + 1));
            yield return new Coords(target.X, (byte)(target.Y - 1));
        }

        public static IEnumerable<ICoords> NobiWalk(BitPlane scope, ICoords target)
        {
            return HortogonalCoord(target).Where(x => scope.InOfRange(x) && scope[x]);
        }
        
    }
}