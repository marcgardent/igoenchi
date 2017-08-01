using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly GroupParser _chainParser;
        private readonly LibertiesParser _libertiesParser;

        public IEnumerable<GoStat> Statistic => _stat;
        
        public Analyser()
        {
            _groupParser = GroupParser.StandardGroupParser();
            _chainParser = GroupParser.NobiGroupParser();

            _libertiesParser = new LibertiesParser();
        }

        public GoStat AnalyseStep(Board board)
        {
            var all = board.Black.Copy().Xor(board.White);
            var blackStat = Build(board.Black, all);
            var whiteStat = Build(board.White, all);
            var current = new GoStat(blackStat, whiteStat, _stat.Count+1);
            var previous = _stat.LastOrDefault();
             _stat.Add(current);

            return current;
        }


        private GoPlayerStat Build(BitPlane me, BitPlane all)
        {
            var libertiesGoban = _libertiesParser.Parse(me, all);
            var ret = new GoPlayerStat(libertiesGoban);
            var groups = _groupParser.Parse(me);

            foreach (var group in groups)
            {
                var liberties = _libertiesParser.Parse(group, all);
                var chains = BuildChain(group, all);
                ret.Groups.Add(new GroupStat(group, liberties, chains));
            }
            
            return ret;
        }

        private Collection<ChainStat> BuildChain(BitPlane group, BitPlane all)
        {
            var ret = new Collection<ChainStat>();
            var chains = _chainParser.Parse(group);
            foreach (var chain in chains)
            {
                var liberties = _libertiesParser.Parse(group, all);
                ret.Add(new ChainStat(chain, liberties));

            }

            return ret;
        }
    }

}