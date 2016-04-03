using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Models;

namespace IGOPhoenix.GoGameAnalytic
{
    public class Analyser
    {
        private readonly List<GoStat> _stat = new List<GoStat>();
        private readonly GroupParser _groupParser;
        private readonly LibertiesParser _libertiesParser;

        public IEnumerable<GoStat> Statistic => _stat;
        
        public Analyser()
        {
            _groupParser = GroupParser.NobiGroupParser();
            _libertiesParser = new LibertiesParser();
        }

        public void AnalyseStep(Board board)
        {
            var all = board.Black.Copy().Xor(board.White);
            var blackStat = Build(board.Black, all);
            var whiteStat = Build(board.White, all);
            var current = new GoStat(blackStat, whiteStat, _stat.Count+1);
            var previous = _stat.LastOrDefault();
             _stat.Add(current);
        }


        private GoPlayerStat Build(BitPlane me, BitPlane all)
        {
            var ret = new GoPlayerStat();
            var groups = _groupParser.Parse(me);

            foreach (var group in groups)
            {
                var liberties = _libertiesParser.Parse(group, all);
                ret.Groups.Add(new GroupStat(group, liberties));
            }
            
            return ret;
        }
    }

}