using System.Collections.Generic;

namespace IGOEnchi.GoGameLogic
{
    public class BoardMarkup
    {
        private readonly byte boardSize;

        public BoardMarkup(byte boardSize)
        {
            Labels = new List<TextLabel>();
            Marks = new List<Mark>();
            this.boardSize = boardSize;
        }

        public Board Territory { get; private set; }

        public BitPlane DeadStones { get; private set; }

        public List<TextLabel> Labels { get; private set; }

        public List<Mark> Marks { get; private set; }

        public void EnsureTerritory()
        {
            if (Territory == null)
            {
                Territory = new Board(boardSize);
            }
        }

        public void FreeTerritory()
        {
            Territory = null;
        }

        public void EnsureDeadStones()
        {
            if (DeadStones == null)
            {
                DeadStones = new BitPlane(boardSize);
            }
        }

        public void FreeDeadStones()
        {
            DeadStones = null;
        }
    }
}