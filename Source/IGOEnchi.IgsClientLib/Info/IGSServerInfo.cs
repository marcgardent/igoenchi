using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IgsServerInfo
    {
        private readonly IGSClient client;
        private readonly List<IGSGameInfoRequest> gameInfoRequests;
        private IgsPlayerInfo info;

        public IgsServerInfo(IGSClient client)
        {
            if (client == null)
            {
                throw new Exception("Argument cannot be null");
            }

            this.client = client;

            client.AddHandler(IGSMessages.GamesList, ReadGamesList);
            client.AddHandler(IGSMessages.PlayersList, ReadPlayersList);
            client.AddHandler(IGSMessages.Info, ReadInfo);

            gameInfoRequests = new List<IGSGameInfoRequest>();
        }

        public IGSGameInfo[] Games { get; private set; }

        public IgsPlayerInfo[] Players { get; private set; }

        public IgsPlayerInfo PlayerStats
        {
            get { return info; }
        }

        public IGSToggleSettings ToggleSettings { get; private set; }

        public event EventHandler ToggleSettingsUpdated;
        public event EventHandler PlayerStatsUpdated;
        public event EventHandler GameListUpdated;
        public event EventHandler PlayersListUpdated;

        public void RequestPlayerStats(string player)
        {
            client.WriteLine("stats " + player);
        }

        public bool RequestPlayersList()
        {
            if (client.WriteLine("user") != null)
            {
                return true;
            }
            return false;
        }

        public bool RequestGamesList()
        {
            if (client.WriteLine("games") != null)
            {
                return true;
            }
            return false;
        }

        public void RequestGameInfo(int gameNumber, EventHandler callback)
        {
            gameInfoRequests.Add(new IGSGameInfoRequest(gameNumber, callback));
            client.WriteLine("games " + gameNumber);
        }

        private void ReadInfo(List<string> lines)
        {
            info = new IgsPlayerInfo();
            if (lines[0].StartsWith("Player:"))
            {
                var firstLine = lines[0];
                var name = firstLine.Substring(
                    firstLine.IndexOf(' '),
                    firstLine.Length - firstLine.IndexOf(' ')).TrimStart(' ');
                if (name == client.CurrentAccount.Name)
                {
                    //9 Verbose  Bell  Quiet  Shout  Automail  Open  Looking  Client  Kibitz  Chatter
                    //9     Off   Off     On     On       Off    On       On      On      On   On
                    var toggles =
                        lines[lines.Count - 1].Replace(" ", "").Split('O');
                    ToggleSettings =
                        new IGSToggleSettings(toggles[6] == "n",
                            toggles[7] == "n",
                            toggles[9] == "n");
                    OnToggleSettingsUpdated(EventArgs.Empty);
                }
                else
                {
                    foreach (var line in lines)
                    {
                        var type = line.Substring(0, line.IndexOf(' '));
                        var data = line.Substring(
                            line.IndexOf(' '),
                            line.Length - line.IndexOf(' ')).TrimStart(' ');
                        switch (type)
                        {
                            case "Player:":
                                info.Name = data;
                                break;
                            case "Language:":
                                info.Language = data;
                                break;
                            case "Rating:":
                                info.Rank = new IGSRank(data.Split(' ')[0]);
                                break;
                            case "Wins:":
                                info.GamesWon = Convert.ToInt32(data);
                                break;
                            case "Losses:":
                                info.GamesLost = Convert.ToInt32(data);
                                break;
                            case "Country:":
                                info.Country = data;
                                break;
                            case "Info:":
                                info.Info = data;
                                break;
                        }
                    }
                    OnPlayerStatsUpdated(EventArgs.Empty);
                }
            }
        }

        protected void OnToggleSettingsUpdated(EventArgs args)
        {
            if (ToggleSettingsUpdated != null)
            {
                ToggleSettingsUpdated(this, args);
            }
        }

        protected void OnPlayerStatsUpdated(EventArgs args)
        {
            if (PlayerStatsUpdated != null)
            {
                PlayerStatsUpdated(this, args);
            }
        }

        private void ReadGamesList(List<string> lines)
        {
            IGSGameInfo gameInfo;
            if (lines.Count == 2)
            {
                var info = lines[1];
                gameInfo = IGSGameInfo.Parse(info);
                IGSGameInfoRequest matchingRequest = null;
                foreach (var request in gameInfoRequests)
                {
                    if (request.GameNumber == gameInfo.GameNumber)
                    {
                        matchingRequest = request;
                    }
                }

                if (matchingRequest != null)
                {
                    matchingRequest.RequestCompleted(gameInfo);
                    gameInfoRequests.Remove(matchingRequest);
                    return;
                }
            }

            Games = new IGSGameInfo[lines.Count - 1];
            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                gameInfo = IGSGameInfo.Parse(line);
                Games[lines.IndexOf(line) - 1] = gameInfo;
            }
            OnGameListUpdated(EventArgs.Empty);
        }

        protected void OnGameListUpdated(EventArgs args)
        {
            if (GameListUpdated != null)
            {
                GameListUpdated(this, args);
            }
        }

        private void ReadPlayersList(List<string> lines)
        {
            Players = new IgsPlayerInfo[lines.Count - 1];
            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                var playerInfo = IgsPlayerInfo.Parse(line);
                Players[lines.IndexOf(line) - 1] = playerInfo;
            }
            OnPlayersListUpdated(EventArgs.Empty);
        }

        protected void OnPlayersListUpdated(EventArgs args)
        {
            if (PlayersListUpdated != null)
            {
                PlayersListUpdated(this, args);
            }
        }
    }
}