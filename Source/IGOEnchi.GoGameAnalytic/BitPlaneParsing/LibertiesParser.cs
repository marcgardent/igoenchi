using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class LibertiesParser
    {
        
        public BitPlane Parse(BitPlane group, BitPlane allStones)
        {
            var  ret = new BitPlane(allStones.Width, allStones.Height);

            foreach (var coords in group.Unabled)
            {
                var liberties = AnalyticHelper.HortogonalCoord(coords).Where(
                        x => allStones.InOfRange(x) && !allStones[x]
                    );
                ret.AddRange(liberties);
            }

            return ret;
        }
    }
}