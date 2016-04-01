namespace IGoEnchi
{
    public struct MatchRequest
    {
        private readonly string opponentName;
        private readonly string color;
        private readonly int boardSize;
        private readonly int baseTime;
        private readonly int byouyomi;

        public MatchRequest(string opponentName,
            string color,
            int boardSize,
            int baseTime,
            int byouyomi)
        {
            this.opponentName = opponentName;
            this.color = color;
            this.boardSize = boardSize;
            this.baseTime = baseTime;
            this.byouyomi = byouyomi;
        }

        public string OpponentName
        {
            get { return opponentName; }
        }

        public string Color
        {
            get { return color; }
        }

        public int BoardSize
        {
            get { return boardSize; }
        }

        public int BaseTime
        {
            get { return baseTime; }
        }

        public int Byouyomi
        {
            get { return byouyomi; }
        }

        public static MatchRequest DefaultRequest(string userName)
        {
            return new MatchRequest(userName, "B", 19, 30, 10);
        }
    }
}