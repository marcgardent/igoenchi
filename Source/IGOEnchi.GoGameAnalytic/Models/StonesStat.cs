using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class StonesStat
    {
        public int StoneCount => Stones.Count;
        public int LibertyCount => Liberties.Count;

        public readonly BitPlane Stones;
        public readonly BitPlane Liberties;

        public StonesStat(BitPlane stones, BitPlane liberties)
        {
            Stones = stones;
            Liberties = liberties;
        }
    }
}