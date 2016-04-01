using System;
using System.Text.RegularExpressions;

namespace IGoEnchi
{
    public struct IgsPlayerInfo
    {
        private int timeIdle;

        public string Name { get; set; }

        public string Info { get; set; }

        public string Country { get; set; }

        public IGSRank Rank { get; set; }

        public int GamesWon { get; set; }

        public int GamesLost { get; set; }

        public int GamePlaying { get; set; }

        public int GameObserving { get; set; }

        public string TimeIdle
        {
            get { return TimeSpan.FromSeconds(timeIdle).ToString(); }
            set
            {
                switch (value[value.Length - 1])
                {
                    case 'm':
                        timeIdle = 60*Convert.ToInt32(value.Substring(0, value.Length - 1));
                        break;
                    case 'h':
                        timeIdle = 360*Convert.ToInt32(value.Substring(0, value.Length - 1));
                        break;
                    default:
                        timeIdle = Convert.ToInt32(value.Substring(0, value.Length - 1));
                        break;
                }
            }
        }

        public string Flags { get; set; }

        public string Language { get; set; }

        public static IgsPlayerInfo Parse(string line)
        {
            var playerInfo = new IgsPlayerInfo();
            var slashRegularExpression = new Regex(@"\s*/\s*");
            var spacesRegularExpression = new Regex(@"\s+");
            try
            {
                var item = line.Substring(0, 26);
                line = line.Remove(0, 28);

                playerInfo.Name = item.Substring(0, 10);
                playerInfo.Name = playerInfo.Name.Substring(playerInfo.Name.LastIndexOf(' ') + 1);
                playerInfo.Info = item.Substring(12);

                item = line.Substring(0, 7);
                line = line.Remove(0, 7);
                playerInfo.Country = item;
                line = slashRegularExpression.Replace(line, " ");

                var data = spacesRegularExpression.Split(line);

                playerInfo.Rank = new IGSRank(data[1]);
                playerInfo.GamesWon = Convert.ToInt32(data[2]);
                playerInfo.GamesLost = Convert.ToInt32(data[3]);
                if (data[4] == "-")
                {
                    playerInfo.GameObserving = 0;
                }
                else
                {
                    playerInfo.GameObserving = Convert.ToInt32(data[4]);
                }
                if (data[5] == "-")
                {
                    playerInfo.GamePlaying = 0;
                }
                else
                {
                    playerInfo.GamePlaying = Convert.ToInt32(data[5]);
                }
                playerInfo.TimeIdle = data[6];
                playerInfo.Flags = data[7];
                playerInfo.Language = data[8];
            }
            catch (FormatException)
            {
                throw new IGSParseException("Corrupted game info: " + line);
            }
            catch (IndexOutOfRangeException)
            {
                throw new IGSParseException("Corrupted game info: " + line);
            }

            return playerInfo;
        }
    }
}