using System.Collections.Generic;
using System.Linq;

namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class GoPlayerStat
    {

        public int StoneCount => Groups.Sum(x => x.StoneCount);

        public int LibertyCount => Groups.Sum(x => x.LibertyCount);

        public readonly List<GroupStat> Groups = new List<GroupStat>();
    }
}