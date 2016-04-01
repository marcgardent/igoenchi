namespace IGoEnchi
{
    public struct NMatchRequest
    {
        private readonly string opponentName;
        private readonly string color;
        private readonly int handicap;
        private readonly int boardSize;
        private readonly int baseTime;
        private readonly int byouyomiTime;
        private readonly int byouyomiStones;

        public NMatchRequest(string opponentName,
            string color,
            int handicap,
            int boardSize,
            int baseTime,
            int byouyomiTime,
            int byouyomiStones)
        {
            this.opponentName = opponentName;
            this.color = color;
            this.handicap = handicap;
            this.boardSize = boardSize;
            this.baseTime = baseTime;
            this.byouyomiTime = byouyomiTime;
            this.byouyomiStones = byouyomiStones;
        }

        public string OpponentName
        {
            get { return opponentName; }
        }

        public string Color
        {
            get { return color; }
        }

        public int Handicap
        {
            get { return handicap; }
        }

        public int BoardSize
        {
            get { return boardSize; }
        }

        public int BaseTime
        {
            get { return baseTime; }
        }

        public int ByouyomiTime
        {
            get { return byouyomiTime; }
        }

        public int ByouyomiStones
        {
            get { return byouyomiStones; }
        }

        public static NMatchRequest DefaultRequest(string userName)
        {
            return new NMatchRequest(userName, "N", 0, 19, 30, 10, 25);
        }
    }
}