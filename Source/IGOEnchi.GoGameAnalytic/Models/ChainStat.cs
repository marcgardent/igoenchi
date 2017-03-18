using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class ChainStat : StonesStat
    {
        public ChainStat(BitPlane stones, BitPlane liberties) : base(stones, liberties)
        {
        }
    }
}