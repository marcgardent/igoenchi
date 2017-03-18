using System.Collections.Generic;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class GroupStat : ChainStat
    {
        public readonly ICollection<ChainStat> Chains;

        public int ChainCount => Chains.Count;

        public GroupStat(BitPlane stones, BitPlane liberties, ICollection<ChainStat> chains) :base(stones, liberties)
        {
            this.Chains = chains;
        }
    }
}