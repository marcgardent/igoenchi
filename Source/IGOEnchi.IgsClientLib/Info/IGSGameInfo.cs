using System;
using System.Text.RegularExpressions;

namespace IGoEnchi
{
    public struct IGSGameInfo
    {
        private byte gameType;

        public int GameNumber { get; set; }

        public string WhitePlayer { get; set; }

        public string BlackPlayer { get; set; }

        public IGSRank WhiteRank { get; set; }

        public IGSRank BlackRank { get; set; }

        public int MovesMade { get; set; }

        public byte BoardSize { get; set; }

        public byte Handicap { get; set; }

        public float Komi { get; set; }

        public int Byouyomi { get; set; }

        public string GameType
        {
            get
            {
                switch (gameType)
                {
                    case 1:
                        return "Teaching";
                    case 2:
                        return "Tournament";
                    default:
                        return "Free";
                }
            }
            set
            {
                switch (value[0])
                {
                    case 'T':
                        gameType = 1;
                        break;
                    case '*':
                        gameType = 2;
                        break;
                    default:
                        gameType = 0;
                        break;
                }
            }
        }

        public int ObserversCount { get; set; }

        public static IGSGameInfo Parse(string line)
        {
            var gameInfo = new IGSGameInfo();
            var regularExpression = new Regex(@"\s+");
            try
            {
                var items = regularExpression.Replace(line.Replace('[', ' ').Replace(']', ' ').
                    Replace('(', ' ').Replace(')', ' '), " ").
                    Substring(1).Split(' ');

                gameInfo.GameNumber = Convert.ToInt32(items[0]);
                gameInfo.WhitePlayer = items[1];
                gameInfo.WhiteRank = new IGSRank(items[2]);
                gameInfo.BlackPlayer = items[4];
                gameInfo.BlackRank = new IGSRank(items[5]);
                gameInfo.MovesMade = Convert.ToInt32(items[6]);
                gameInfo.BoardSize = Convert.ToByte(items[7]);
                gameInfo.Handicap = Convert.ToByte(items[8]);

                gameInfo.Komi = Convert.ToSingle(items[9].Split('.')[0]);
                gameInfo.Komi += Math.Sign(gameInfo.Komi)*Convert.ToSingle(items[9].Split('.')[1])/10;

                gameInfo.Byouyomi = Convert.ToInt32(items[10]);
                gameInfo.GameType = items[11];
                gameInfo.ObserversCount = Convert.ToInt32(items[12]);
            }
            catch (FormatException)
            {
                throw new IGSParseException("Corrupted game info: " + line);
            }
            catch (IndexOutOfRangeException)
            {
                throw new IGSParseException("Corrupted game info: " + line);
            }

            return gameInfo;
        }
    }
}