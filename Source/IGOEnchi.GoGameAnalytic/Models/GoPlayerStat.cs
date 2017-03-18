using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class GoPlayerStat
    {
        public readonly BitPlane Liberties;

        public int StoneCount => Groups.Sum(x => x.StoneCount);
        
        public int LibertyCount => Liberties.Count;
        
        public float  EfficiencyOfLiberties => LibertyCount / (StoneCount*2f+2f) * 100f;

        public int ChainCount => Groups.Sum(x => x.ChainCount);

        public int GroupCount => Groups.Count;

        public readonly List<GroupStat> Groups = new List<GroupStat>();

        public GoPlayerStat(BitPlane liberties)
        {
            Liberties = liberties;
        }
    }
}