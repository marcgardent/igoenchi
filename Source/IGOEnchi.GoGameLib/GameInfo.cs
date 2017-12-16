namespace IGOEnchi.GoGameLogic
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

        /// <summary>
        /// Rank White Player
        /// map to BR
        /// </summary>
        public string WhiteRank { get; set; }

        /// <summary>
        /// Rank Black Player
        /// map to WR
        /// </summary>
        public string BlackRank { get; set; }

        /// <summary>
        /// Name of game
        /// Todo impl  : map to GN
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// some background information and/or to summarize the game itself
        /// TODO impl : map to GC
        /// </summary>
        public string Summarize { get; set; }



        /// <summary>
        /// TODO impl : map to DT
        /// </summary>
        public string DateOfGame { get; set; }


        /// <summary>
        /// TODO impl : map to RU
        /// </summary>
        public string Rules { get; set; }


        /// <summary>
        /// TODO impl : map to AP [name:version]
        /// the name of the application used to create this gametree
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// TODO impl : map to AP [name:version]
        ///  version number of the application used to create this gametree
        /// </summary>
        public string ProgramVersion { get; set; }
    }
}