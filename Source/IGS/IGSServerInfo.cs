using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IGoEnchi
{		
	
	public struct IGSPlayerInfo
	{
		private string name;
		private string info;
		private string country;
		private IGSRank rank;
		private int gamesWon;
		private int gamesLost;		
		private int gamePlaying;
		private int gameObserving;
		private int timeIdle;
		private string flags;
		private string language;
		
		public string Name
		{
			get {return name;}
			set {name = value;}
		}
		
		public string Info
		{
			get {return info;}
			set {info = value;}
		}
		
		public string Country
		{
			get {return country;}
			set {country = value;}
		}
		
		public IGSRank Rank
		{
			get {return rank;}
			set {rank = value;}
		}
		
		public int GamesWon
		{
			get {return gamesWon;}
			set {gamesWon = value;}
		}
		
		public int GamesLost
		{
			get {return gamesLost;}
			set {gamesLost = value;}
		}
		
		public int GamePlaying
		{
			get {return gamePlaying;}
			set {gamePlaying = value;}
		}
		
		public int GameObserving
		{
			get {return gameObserving;}
			set {gameObserving = value;}
		}
		
		public string TimeIdle
		{
			get 
			{
				return TimeSpan.FromSeconds(timeIdle).ToString() ;
			}
			set 
			{
				switch (value[value.Length - 1])
				{
					case 'm': 
						timeIdle = 60 * Convert.ToInt32(value.Substring(0, value.Length - 1));
						break;
					case 'h': 
						timeIdle = 360 * Convert.ToInt32(value.Substring(0, value.Length - 1));
						break;
					default: 
						timeIdle = Convert.ToInt32(value.Substring(0, value.Length - 1));
						break;
				}
			}
		}
		
		public string Flags
		{
			get {return flags;}
			set {flags = value;}
		}
		
		public string Language
		{
			get {return language;}
			set {language = value;}
		}
		
		public static IGSPlayerInfo Parse(string line)
		{
			IGSPlayerInfo playerInfo = new IGSPlayerInfo();		
			Regex slashRegularExpression = new Regex(@"\s*/\s*");
			Regex spacesRegularExpression = new Regex(@"\s+");	
			try
			{								
				string item = line.Substring(0, 26);
				line = line.Remove(0, 28);
					
				playerInfo.Name = item.Substring(0, 10);
				playerInfo.Name = playerInfo.Name.Substring(playerInfo.Name.LastIndexOf(' ') + 1);							
				playerInfo.Info = item.Substring(12);

				item = line.Substring(0, 7);
				line = line.Remove(0, 7);
				playerInfo.Country = item;
				line = slashRegularExpression.Replace(line, " ");	
					
				string[] data = spacesRegularExpression.Split(line);
				
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
		
	public enum IGSRankClass {Kyu, Dan, Pro};
	
	public class IGSRank: IComparable		
	{
		private int rankValue;
		private IGSRankClass rankClass;
		private string rankString;
			
		public IGSRank(string rank)
		{
			if (rank == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			rankString = rank;
			ParseString(rank);
		}
		
		private void ParseString(string rank)
		{
			int index = 0;
			
			rankValue = Int32.MaxValue;
			rankClass = IGSRankClass.Kyu;
			
			while ((index < rank.Length) && (Char.IsDigit(rank[index])))
			{
				index += 1;
			}
			
			if (index > 0)
			{
				rankValue = Convert.ToInt32(rank.Substring(0, index));
				if (index < rank.Length)
				{
					switch (rank[index])
					{
						case 'k': rankClass = IGSRankClass.Kyu;
							break;
						case 'd': rankClass = IGSRankClass.Dan;
							break;
						case 'p': rankClass = IGSRankClass.Pro;
							break;
					}
				}
			}
		}
		
		public override string ToString()
		{
			return rankString;
		}
		
		public int CompareTo(object rank)
		{			
			if (!(rank is IGSRank))
			{
				throw new ArgumentException("Argument is not of type IGSRank");
			}
			IGSRank otherRank = rank as IGSRank;
			
			if (rankClass != otherRank.rankClass)
			{
				switch (rankClass)
				{
					case IGSRankClass.Pro: return 1;
					case IGSRankClass.Kyu: return -1;
					default:
						switch (otherRank.rankClass)
						{
							case IGSRankClass.Pro: return -1;
							case IGSRankClass.Kyu: return 1;	
						}
						break;						
				}
			}
			else
			{
				if (rankClass == IGSRankClass.Kyu)
				{
					return (- rankValue).CompareTo(- otherRank.rankValue);
				}
				else
				{
					return rankValue.CompareTo(otherRank.rankValue);
				}
			}
			
			return 0;
		}
	}
	
	public struct IGSGameInfo
	{
		private int gameNumber;		
		private string whitePlayer;
		private string blackPlayer;
		private IGSRank whiteRank;
		private IGSRank blackRank;
		private int movesMade;
		private byte boardSize;
		private byte handicap;
		private float komi;
		private int byouyomi;
		private byte gameType;
		private int observersCount;
		
		public int GameNumber
		{
			get {return gameNumber;}
			set {gameNumber = value;}
		}
		
		public string WhitePlayer
		{
			get {return whitePlayer;}
			set {whitePlayer = value;}
		}
		
		public string BlackPlayer
		{
			get {return blackPlayer;}
			set {blackPlayer = value;}
		}
		
		public IGSRank WhiteRank
		{
			get {return whiteRank;}
			set {whiteRank = value;}
		}
		
		public IGSRank BlackRank
		{
			get {return blackRank;}
			set {blackRank = value;}
		}
		
		public int MovesMade
		{
			get {return movesMade;}
			set {movesMade = value;}
		}
		
		public byte BoardSize
		{
			get {return boardSize;}
			set {boardSize = value;}
		}
		
		public byte Handicap
		{
			get {return handicap;}
			set {handicap = value;}
		}
		
		public float Komi
		{
			get {return komi;}
			set {komi = value;}
		}
		
		public int Byouyomi
		{
			get {return byouyomi;}
			set {byouyomi = value;}
		}
		
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
		
		public int ObserversCount
		{
			get {return observersCount;}
			set {observersCount = value;}
		}
		
		public static IGSGameInfo Parse(string line)
		{
			IGSGameInfo gameInfo = new IGSGameInfo();
			Regex regularExpression = new Regex(@"\s+");
			try
			{					
				string[] items = regularExpression.Replace(line.Replace('[', ' ').Replace(']', ' ').
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
				
				gameInfo.Komi =  Convert.ToSingle(items[9].Split('.')[0]);
				gameInfo.Komi += Math.Sign(gameInfo.Komi) * Convert.ToSingle(items[9].Split('.')[1]) / 10;
					
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
		
	public class IGSGameInfoRequest
	{
		private int gameNumber;		
		private EventHandler callback;		
		private IGSGameInfo result;
		
		public int GameNumber
		{
			get {return gameNumber;}
		}
		
		public IGSGameInfo Result
		{
			get {return result;}
		}
		
		public IGSGameInfoRequest(int gameNumber, EventHandler callback)
		{
			if (callback == null)
			{
				throw new ArgumentException("EventHandler argument cannot be null");
			}
			this.gameNumber = gameNumber;
			this.callback = callback;
		}
		
		public void RequestCompleted(IGSGameInfo result)
		{
			this.result = result;
			callback(this, new EventArgs());
		}
	}
	
	public class IGSServerInfo
	{
		private IGSClient client;	
		private IGSGameInfo[] games;
		private IGSPlayerInfo[] players;
		private IGSPlayerInfo info;
		private IGSToggleSettings toggleSettings;
		private List<IGSGameInfoRequest> gameInfoRequests;		
		
		public event EventHandler ToggleSettingsUpdated;
		public event EventHandler PlayerStatsUpdated;
		public event EventHandler GameListUpdated;
		public event EventHandler PlayersListUpdated;		
		
		public IGSGameInfo[] Games
		{
			get {return games;}			
		}
		
		public IGSPlayerInfo[] Players
		{
			get {return players;}
		}
		
		public IGSPlayerInfo PlayerStats
		{
			get {return info;}
		}
		
		public IGSToggleSettings ToggleSettings
		{
			get {return toggleSettings;}
		}
		
		public IGSServerInfo(IGSClient client)
		{
			if (client == null)
			{
				throw new Exception("Argument cannot be null");
			}
			
			this.client = client;
						
			client.AddHandler(IGSMessages.GamesList, new IGSMessageHandler(ReadGamesList));
			client.AddHandler(IGSMessages.PlayersList, new IGSMessageHandler(ReadPlayersList));
			client.AddHandler(IGSMessages.Info, new IGSMessageHandler(ReadInfo));
			
			gameInfoRequests = new List<IGSGameInfoRequest>();
		}
				
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
			else
			{
				return false;
			}
		}
		
		public bool RequestGamesList()
		{			
			if (client.WriteLine("games") != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		public void RequestGameInfo(int gameNumber, EventHandler callback)
		{
			gameInfoRequests.Add(new IGSGameInfoRequest(gameNumber, callback));
			client.WriteLine("games " + gameNumber);
		}
		
		private void ReadInfo(List<string> lines)
		{
			info = new IGSPlayerInfo();
			if (lines[0].StartsWith("Player:"))
			{
				var firstLine = lines[0];
				var name = firstLine.Substring(
						firstLine.IndexOf(' '),
					   	firstLine.Length - firstLine.IndexOf(' ')).TrimStart(new char[] {' '});
				if (name == client.CurrentAccount.Name)
				{
					//9 Verbose  Bell  Quiet  Shout  Automail  Open  Looking  Client  Kibitz  Chatter
					//9     Off   Off     On     On       Off    On       On      On      On   On
					var toggles = 
						lines[lines.Count - 1].Replace(" ", "").Split(new char[] {'O'});
					toggleSettings = 
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
							line.Length - line.IndexOf(' ')).TrimStart(new char[] {' '});
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
											
			games = new IGSGameInfo[lines.Count - 1];	
			for (var i = 1; i < lines.Count; i++)
			{				
				var line = lines[i];
				gameInfo = IGSGameInfo.Parse(line);				
				games[lines.IndexOf(line) - 1] = gameInfo;
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
			players = new IGSPlayerInfo[lines.Count - 1];
			for (var i = 1; i < lines.Count; i++)
			{				
				var line = lines[i];
				IGSPlayerInfo playerInfo = IGSPlayerInfo.Parse(line);
				players[lines.IndexOf(line) - 1] = playerInfo;
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




