using System.Collections.Generic;
using System.ComponentModel;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.GoGameAnalytic
{

    public class Group
    {
        public int NumberOfStone => Stones.Count;
        public int NumberOfliberty => Liberties.Count;
        public int NumberOfCutting => CutStones.Count;

        public List<ICoords> Stones = new List<ICoords>();
        public List<ICoords> Liberties = new List<ICoords>();
        public List<ICoords> CutStones = new List<ICoords>();
    }
}
