using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
 
namespace IGoEnchi
{
	public struct MatchRequest
	{
		private string opponentName;
		private string color; 
		private int boardSize;
		private int baseTime;
		private int byouyomi;
		
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
			get {return opponentName;}
		}
		
		public string Color
		{
			get {return color;}
		}
		
		public int BoardSize
		{
			get {return boardSize;}
		}
		
		public int BaseTime
		{
			get {return baseTime;}
		}
		
		public int Byouyomi
		{
			get {return byouyomi;}
		}
		
		public static MatchRequest DefaultRequest(string userName)
		{
			return new MatchRequest(userName, "B", 19, 30, 10);			
		}
	}
		
	public struct NMatchRequest
	{
		private string opponentName;
		private string color; 
		private int handicap;
		private int boardSize;
		private int baseTime;
		private int byouyomiTime;
		private int byouyomiStones;
		
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
			get {return opponentName;}
		}
		
		public string Color
		{
			get {return color;}
		}
		
		public int Handicap
		{
			get {return handicap;}
		}
		
		public int BoardSize
		{
			get {return boardSize;}
		}
		
		public int BaseTime
		{
			get {return baseTime;}
		}
		
		public int ByouyomiTime
		{
			get {return byouyomiTime;}
		}
		
		public int ByouyomiStones
		{
			get {return byouyomiStones;}
		}
		
		public static NMatchRequest DefaultRequest(string userName)
		{
			return new NMatchRequest(userName, "N", 0, 19, 30, 10, 25);
		}
	}
	
	public struct SeekRequest
	{
		private int timeEntry;
		private int boardSize;
		private int handicap;
		
		private static string[] timeLabels = 
		{"1 min + 10 min / 25 stones",
	     "1 min + 7 min / 25 stones",
	     "1 min + 5 min / 25 stones",
	     "1 min + 15 min / 25 stones"};
		
		public SeekRequest(int timeEntry, int boardSize, int handicap)
		{
			this.timeEntry = timeEntry;
			this.boardSize = boardSize;
			this.handicap = handicap;
		}
		
		public static string[] TimeLabels
		{
			get {return timeLabels;}
		}
		
		public int TimeEntry
		{
			get {return timeEntry;}
		}
		
		public int BoardSize
		{
			get {return boardSize;}
		}
		
		public int Handicap
		{
			get {return handicap;}
		}
	}
	
	public class IGSPlayer: IGSGameListener
	{	
		private MatchRequest incomingRequest;
		private NMatchRequest incomingNRequest;
		private SeekRequest lastSeekRequest;
		private string[] storedGames;
		private ObservedGame saySource;
		
		public event EventHandler MatchRequestReceived;
		public event EventHandler NMatchRequestReceived;
		public event EventHandler StoredGamesReceived;
		public event IGSMessageHandler SayReceived;
		
		public event EventHandler SeekStarted;
		public event EventHandler SeekEnded;
		
		public IGSPlayer(IGSClient client, IGSServerInfo serverInfo): base(client, serverInfo) 
		{	
			client.AddHandler(IGSMessages.SaySource, ReadSaySource);
			client.AddHandler(IGSMessages.Say, ReadSay);
			client.AddHandler(IGSMessages.Info, ReadInfo);
			client.AddHandler(IGSMessages.Adjourn, ReadAdjourn);
			client.AddHandler(IGSMessages.Score, ReadScore);
			client.AddHandler(IGSMessages.StoredGames, ReadStoredGames);
			client.AddHandler(IGSMessages.Undo, ReadUndo);
			client.AddHandler(IGSMessages.StoneRemoval, ReadStoneRemoval);
			client.AddHandler(IGSMessages.SeekInfo, ReadSeekInfo);
		}

		public string[] StoredGames
		{
			get {return storedGames;}
		}
		
		public MatchRequest IncomingRequest
		{
			get {return incomingRequest;}
		}
		
		public NMatchRequest IncomingNRequest
		{
			get {return incomingNRequest;}
		}
		
		public SeekRequest LastSeekRequest
		{
			get {return lastSeekRequest;}
		}
		
		public void Match(MatchRequest request)
		{ 									
			if (Games.Count > 0)
			{
				MessageBox.Show("Sorry, but playing multiple games isn't supported yet",
					            "Note",
					            MessageBoxButtons.OK,
					            MessageBoxIcon.Asterisk,
					            MessageBoxDefaultButton.Button1);
				return;
			}
			Client.WriteLine("match " + 
			                 request.OpponentName + " " +
			                 request.Color + " " +
			                 request.BoardSize + " " +
			                 request.BaseTime + " " +
			                 request.Byouyomi);
		}
		
		public void NMatch(NMatchRequest request)
		{ 									
			if (Games.Count > 0)
			{
				MessageBox.Show("Sorry, but playing multiple games isn't supported yet",
					            "Note",
					            MessageBoxButtons.OK,
					            MessageBoxIcon.Asterisk,
					            MessageBoxDefaultButton.Button1);
				return;
			}
			Client.WriteLine("nmatch " + 
			                 request.OpponentName + " " +
			                 request.Color + " " +
			                 request.Handicap + " " +
			                 request.BoardSize + " " +
			                 request.BaseTime + " " +
			                 request.ByouyomiTime + " " +
			                 request.ByouyomiStones + " 0 0 0");
		}
		
		public void Seek(SeekRequest request)
		{
			Client.WriteLine("seek entry " + 
			                 request.TimeEntry + " " +
			                 request.BoardSize + " " +
			                 request.Handicap + " " +
			                 request.Handicap + " 0");
		}
		
		public void CancelSeek()
		{
			Client.WriteLine("seek entry_cancel");
		}
		
		public void PlaceStone(ObservedGame game, Stone stone)
		{			
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			var command = "";
			if (stone.X < 8)
			{
				command += Convert.ToChar('A' + stone.X);
			}
			else
			{
				command += Convert.ToChar('B' + stone.X);
			}
			command += Convert.ToString(game.BoardSize - stone.Y) + " " + game.GameNumber;
			Client.WriteLine(command);
		}
		
		protected override void ReadMoves(List<string> lines)
		{
			var regex = new Regex(@"\s" + Client.CurrentAccount.Name + @"\s");			
			if (regex.IsMatch(lines[0]))
			{
				base.ReadMoves(lines);
			}
		}
		
		public void Resign(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("resign " + game.GameNumber);
		}
		
		public void Pass(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("pass " + game.GameNumber);
		}
		
		public void Adjourn(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("adjourn " + game.GameNumber);
		}
		
		public void Done(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("done " + game.GameNumber);
		}
		
		public void Undo(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("undo " + game.GameNumber);
		}
		
		public void AskUndo(ObservedGame game)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			Client.WriteLine("undoplease " + game.GameNumber);
		}
		
		public void Say(ObservedGame game, string message)
		{
			if (game == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			if (Games.Contains(game))
			{
				Client.WriteLine("say " + message);
			}
		}
		
		public void LoadGame(string name)
		{
			Client.WriteLine("load " + name);
		}
		
		public void RequestStoredGames()
		{
			Client.WriteLine("stored");
		}
		
		private void ReadSaySource(List<string> lines)
		{
			try
			{
				var gameNumber = Convert.ToInt32(lines[0].Substring(lines[0].LastIndexOf(' ') + 1));
				foreach (var game in Games)
				{
					if (game.GameNumber == gameNumber)
					{
						saySource = game;
					}
				}
			}
			catch (FormatException)
			{
				throw new IGSParseException("Corrupted message: " + (lines[0] as string));
			}
			catch (IndexOutOfRangeException)
			{
				
			}
		}

		private void ReadSay(List<string> lines)
		{
			OnSayReceived(lines);
		}
		
		private void ReadScore(List<string> lines)
		{
			var score = lines[0];
			if (Games.Count > 0)
			{
				OnGameEnded(new IGSGameEventArgs(Games[0]));
				Games.Remove(Games[0]);
			}
			MessageBox.Show(score.Replace(" (W:O)", "").Replace(" (B:#)", ""),
			                "Info",
			                MessageBoxButtons.OK,
			                MessageBoxIcon.Asterisk,
			                MessageBoxDefaultButton.Button1);			
		}
		
		private void ReadStoneRemoval(List<string> lines)
		{		
			var gameNumber = 0;
			var groupCoord = "";
			try
			{
				var data = lines[0].Split(' ');
				gameNumber = Convert.ToInt32(data[1]);
				groupCoord = data[6];
			}
			catch (FormatException)
			{
				throw new IGSParseException("Corrupted group removal message: " + lines[0] as string);	
			}
			catch (IndexOutOfRangeException)
			{
				throw new IGSParseException("Corrupted group removal message: " + lines[0] as string);
			}
			
			foreach (var game in Games)
			{
				if (game.GameNumber == gameNumber)
				{
					var stone = new Stone();
					if (groupCoord[0] < 'I')
					{	
						stone.X = Convert.ToByte(groupCoord[0] - 'A');
					}
					else
					{
						stone.X = Convert.ToByte(groupCoord[0] - 'A' - 1);
					}
					stone.Y = Convert.ToByte(game.BoardSize - Convert.ToInt32(groupCoord.Substring(1)));
					game.CurrentNode.MarkDead(stone);
					OnGameChanged(new IGSGameEventArgs(game));
				}
			}			
		}
		
		private void ReadUndo(List<string> lines)
		{
			foreach (var line in lines)
			{		
				if (line.IndexOf("undid the last move") > 0)
				{
					if (Games.Count > 0)
					{	
						(Games[0] as ObservedGame).ToPreviousMove();
						(Games[0] as ObservedGame).UndoMade();
						OnGameChanged(new IGSGameEventArgs((Games[0] as ObservedGame)));
					}	
				}
			}			
		}
		
		private void ReadAdjourn(List<string> lines)
		{
			var data = lines[0].Split(' ');
			var gameNumber = 0;
			
			try
			{
				gameNumber = Convert.ToInt32(data[1]);
			}
			catch (FormatException)
			{
				throw new IGSParseException("CorruptedAdjournRequest: " + (lines[0] as string));
			}
			catch (IndexOutOfRangeException)
			{
				throw new IGSParseException("CorruptedAdjournRequest: " + (lines[0] as string));
			}
			
			if (lines[0].EndsWith("requests an adjournment"))
			{
				if (MessageBox.Show(data[2] + " requests an adjournment. Do you agree?", 
				                "Game " + gameNumber, 
				                MessageBoxButtons.YesNo,
				                MessageBoxIcon.Question,
				                MessageBoxDefaultButton.Button1) == DialogResult.Yes)
				{
					Client.WriteLine("adjourn " + gameNumber);
				}	
				else
				{
					Client.WriteLine("decline adjourn");
				}
			}
			else
			{				
				MessageBox.Show(lines[0],
				                "Game " + gameNumber, 
				                MessageBoxButtons.YesNo,
				                MessageBoxIcon.Exclamation,
				                MessageBoxDefaultButton.Button1);							
			}
		}
		
		private void ReadSeekInfo(List<string> lines)			
		{			
			for (var i = 0; i < lines.Count; i++)
			{
				var data = lines[i].Split(' ');				
				if (data.Length < 1)
				{
					throw new IGSParseException("Corrupted seek data: " + (lines[i] as string));
				}
				switch (data[0])
				{
					case "ENTRY":				
						var time = 0;
						switch (Convert.ToInt32(data[2]))
						{
							case 420:
								time = 1;
								break;
							case 300:
								time = 2;
								break;
							case 900:
								time = 3;
								break;
						}
						lastSeekRequest = new SeekRequest(time,
						                                  Convert.ToInt32(data[6]),
						                                  Convert.ToInt32(data[7]));
						OnSeekStarted(EventArgs.Empty);
						break;
					case "ENTRY_CANCEL":						
						OnSeekEnded(EventArgs.Empty);
						break;
					case "OPPONENT_FOUND":
						OnSeekEnded(EventArgs.Empty);
						MessageBox.Show("Opponent found!",
				            		    "Seek status", 
				                		MessageBoxButtons.OK,
				                		MessageBoxIcon.Asterisk,
				                		MessageBoxDefaultButton.Button1);
						break;
				}
			}
		}
		
		private void ReadStoredGames(List<string> lines)
		{
			var list = new List<string>();
			var regex = new Regex(@"\s+");	
						
			for (var i = 0; i < lines.Count - 1; i++)
			{
				var items = regex.Split(lines[i]);
				foreach (var item in items)
				{
					if (item != "" )
					{
						list.Add(item);
					}
				}
			}
			storedGames = new string[list.Count];
			for (var i = 0; i < list.Count; i++)
			{
				storedGames[i] = list[i];
			}
			OnStoredGamesReceived(new EventArgs());
		}
		
		private void ReadInfo(List<string> lines)
		{
			if (lines[0].StartsWith("Match["))
			{
				var data = lines[1];				
				Console.WriteLine(data);
				data = data.Substring(data.IndexOf('<') + 1, data.IndexOf('>') - data.IndexOf('<') - 1);
				Console.WriteLine(data);
				var items = data.Split(' ');
				incomingRequest = new MatchRequest(items[1],
				                                        items[2],
				                                        Convert.ToInt32(items[3]),
				                                        Convert.ToInt32(items[4]),
				                                        Convert.ToInt32(items[5]));
				OnMatchRequestReceived(new EventArgs());
			}
			else if (lines[0].StartsWith("NMatch "))
			{
				var data = lines[0];				
				var name = data.Remove(0, 22);
				name = name.Substring(0, name.IndexOf('('));
				Console.WriteLine(data);
				data = data.Substring(data.IndexOf('(') + 1, data.IndexOf(')') - data.IndexOf('(') - 1);
				Console.WriteLine(data);
				var items = data.Split(' ');
				
				incomingNRequest = new NMatchRequest(name,
				                                   items[0],
				                                   Convert.ToInt32(items[1]),
				                                   Convert.ToInt32(items[2]),
				                                   Convert.ToInt32(items[3]),
				                                   Convert.ToInt32(items[4]),
				                                   Convert.ToInt32(items[5]));
				OnNMatchRequestReceived(new EventArgs());
			}
			else if (lines[0].EndsWith("Game has been adjourned.") ||			        
			         lines[0].EndsWith("has resigned the game."))				
			{
				OnGameEnded(new IGSGameEventArgs(Games[0]));
				Games.Remove(Games[0]);				
				MessageBox.Show(lines[0],
				                "Info", 
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Asterisk,
				                MessageBoxDefaultButton.Button1);
			}
			else if (lines[0].EndsWith("has restored your old game.") ||
			         lines[0].EndsWith("declines your request for a match.") ||			         
			         lines[0].EndsWith("has typed done.") ||
			         lines[0].StartsWith("Set the komi to") ||
			         (lines[0].IndexOf("wants the komi to be") >= 0))
			{			
				MessageBox.Show(lines[0],
				                "Info", 
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Asterisk,
				                MessageBoxDefaultButton.Button1);
			}
			else if (lines[0].EndsWith("Board is restored to what it was when you started scoring"))
			{
				if (Games.Count > 0)
				{
					Games[0].CurrentNode.Markup.FreeDeadStones();
					OnGameChanged(new IGSGameEventArgs(Games[0]));
				}
			}			
		}
		
		protected void OnMatchRequestReceived(EventArgs args)
		{
			if (MatchRequestReceived != null)
			{
				MatchRequestReceived(this, args);
			}
		}
		
		protected void OnNMatchRequestReceived(EventArgs args)
		{
			if (NMatchRequestReceived != null)
			{
				NMatchRequestReceived(this, args);
			}
		}
		
		protected void OnSeekStarted(EventArgs args)
		{
			if (SeekStarted != null)
			{
				SeekStarted(this, args);
			}
		}
		
		protected void OnSeekEnded(EventArgs args)
		{
			if (SeekEnded != null)
			{
				SeekEnded(this, args);
			}
		}
		
		protected void OnStoredGamesReceived(EventArgs args)
		{
			if (StoredGamesReceived != null)
			{
				StoredGamesReceived(this, args);
			}
		}
		
		protected void OnSayReceived(List<string> lines)
		{
			if (SayReceived != null)
			{
				SayReceived(lines);
			}
		}
		
		public void Decline(string userName)
		{			
			Client.WriteLine("decline " + userName);			
		}
		
		public string GetOpponentName(ObservedGame game)
		{
			if (!Games.Contains(game))
			{
				throw new ArgumentException("The game isn't played");
			}
			
			if (game.Info.BlackPlayer == Client.CurrentAccount.Name)
			{
				return game.Info.WhitePlayer;
			}
			else if (game.Info.WhitePlayer == Client.CurrentAccount.Name)
			{
				return game.Info.BlackPlayer;
			}	
			else
			{
				throw new ArgumentException("The game isn't played");
			}
		}
		
		public void SetKomi(float komi)
		{
			Client.WriteLine("komi " + komi);
		}
		
		public void SetHandicap(int handicap)
		{
			Client.WriteLine("handicap " + handicap);
		}
	}
}

