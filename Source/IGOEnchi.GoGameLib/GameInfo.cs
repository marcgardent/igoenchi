namespace IGOEnchi.GoGameLogic.Models
{
    public class GameInfo
    {
        public GameInfo()
        {
            BlackPlayer = "black";
            WhitePlayer = "white";
            Handicap = 0;
            Komi = 0;
        }

        public GameInfo(string blackPlayerName, string whitePlayerName, int handiInt, float komiFloat)
        {
            BlackPlayer = blackPlayerName;
            WhitePlayer = whitePlayerName;
            Handicap = handiInt;
            Komi = komiFloat;
        }


        public string BlackPlayer { get; set; }

        public string WhitePlayer { get; set; }

        public int Handicap { get; set; }

        public float Komi { get; set; }
    }
}