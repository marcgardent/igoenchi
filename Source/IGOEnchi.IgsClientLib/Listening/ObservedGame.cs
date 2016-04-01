using System;
using IGOEnchi.GoGameLogic.Models;

namespace IGoEnchi
{
    public class ObservedGame : GoGame
    {
        public ObservedGame(byte boardSize, int gameNumber) : base(boardSize)
        {
            MovesMade = 0;
            GameNumber = gameNumber;
        }

        public int GameNumber { get; private set; }

        public int MovesMade { get; private set; }

        public TimeSpan BlackTime { get; set; }

        public TimeSpan WhiteTime { get; set; }

        public int BlackByouyomiStones { get; set; }

        public int WhiteByouyomiStones { get; set; }

        public int BlackCaptures { get; set; }

        public int WhiteCaptures { get; set; }

        public void MoveMade()
        {
            MovesMade = MovesMade + 1;
        }

        public void UndoMade()
        {
            MovesMade = MovesMade - 1;
        }
    }
}