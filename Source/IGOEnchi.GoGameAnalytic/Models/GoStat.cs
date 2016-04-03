namespace IGOPhoenix.GoGameAnalytic.Models
{
    public class GoStat
    {
        public readonly int Turn;
        public readonly GoPlayerStat BlackStat;
        public readonly GoPlayerStat WhiteStat;

        public GoStat(GoPlayerStat black, GoPlayerStat white, int turn)
        {
            BlackStat = black;
            WhiteStat = white;
            Turn = turn;
        }
    }
}
