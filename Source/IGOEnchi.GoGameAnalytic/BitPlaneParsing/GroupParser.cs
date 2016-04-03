using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class GroupParser
    {
        public static GroupParser NobiGroupParser()
        {
            return new GroupParser(AnalyticHelper.NobiWalk);
        }

        private readonly AnalyticHelper.WalkHandler _walk;
        
        public GroupParser(AnalyticHelper.WalkHandler walk)
        {
            _walk = walk;
        }

        public List<BitPlane> ParseGroups(BitPlane stones)
        {
            var ret = new List<BitPlane>();
            var scope = stones.Copy();

            while (scope.Unabled.Any())
            {
                var grp = GroupWalker(scope, scope.Unabled.First());
                var bp = new BitPlane(stones.Width, stones.Height);
                bp.AddRange(grp);
                ret.Add(bp);
            }

            return ret;
        }

        private IEnumerable<ICoords> GroupWalker(BitPlane scope, ICoords fuse)
        {
            var todo = new List<ICoords>();
            todo.Add(fuse);

            while (todo.Any())
            {
                var target = todo.First();
                yield return target;
                scope.Remove(target);
                todo.Remove(target);
                todo.AddRange(_walk(scope, target));
            }
        }
    }
}