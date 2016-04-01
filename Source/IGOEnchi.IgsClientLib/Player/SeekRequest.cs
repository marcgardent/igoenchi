namespace IGoEnchi
{
    public struct SeekRequest
    {
        private readonly int timeEntry;
        private readonly int boardSize;
        private readonly int handicap;

        private static readonly string[] timeLabels =
        {
            "1 min + 10 min / 25 stones",
            "1 min + 7 min / 25 stones",
            "1 min + 5 min / 25 stones",
            "1 min + 15 min / 25 stones"
        };

        public SeekRequest(int timeEntry, int boardSize, int handicap)
        {
            this.timeEntry = timeEntry;
            this.boardSize = boardSize;
            this.handicap = handicap;
        }

        public static string[] TimeLabels
        {
            get { return timeLabels; }
        }

        public int TimeEntry
        {
            get { return timeEntry; }
        }

        public int BoardSize
        {
            get { return boardSize; }
        }

        public int Handicap
        {
            get { return handicap; }
        }
    }
}