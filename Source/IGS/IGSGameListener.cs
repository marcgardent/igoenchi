using System;
using System.Collections.Generic;
using System.Windows.Forms;
 
namespace IGoEnchi
{
	public class IGSGameEventArgs: EventArgs
	{
		private ObservedGame game;
			
		public ObservedGame Game
		{
			get {return game;}
		}
		
		public IGSGameEventArgs(ObservedGame game)
		{
			this.game = game;
		}
	}
	
	public delegate void IGSGameEventHandler(object sender, IGSGameEventArgs args);
	
	public struct IGSGameStatus
	{
		public const int Black = 0;
		public const int White = 1; 
		public const int Empty = 2; 
		public const int Dame = 3; 
		public const int WhiteTerritory = 4; 
		public const int BlackTerritory = 5; 
		public const int Hoshi = 6; 
		public const int Counted = 7; 

	}	        
	
	public class ObservedGame: GoGame
	{
		private int gameNumber;
		private int movesMade;
		private TimeSpan blackTime;
		private TimeSpan whiteTime;
		private int blackByouyomiStones;
		private int whiteByouyomiStones;
		private int blackCaptures;
		private int whiteCaptures;				
		
		public ObservedGame(byte boardSize, int gameNumber): base(boardSize)
		{			
			movesMade = 0;
			this.gameNumber = gameNumber;			
		}
					
		public void MoveMade()
		{
			movesMade = movesMade + 1;
		}
		
		public void UndoMade()
		{
			movesMade = movesMade - 1;
		}
		
		public int GameNumber
		{
			get {return gameNumber;}
		}
		
		public int MovesMade
		{
			get {return movesMade;}
		}
		
		public TimeSpan BlackTime
		{
			get {return blackTime;}
			set {blackTime = value;}
		}
		
		public TimeSpan WhiteTime
		{
			get {return whiteTime;}			
			set {whiteTime = value;}
		}
		
		public int BlackByouyomiStones
		{
			get {return blackByouyomiStones;}
			set {blackByouyomiStones = value;}
		}
		
		public int WhiteByouyomiStones
		{
			get {return whiteByouyomiStones;}
			set {whiteByouyomiStones = value;}
		}
		
		public int BlackCaptures
		{
			get {return blackCaptures;}
			set {blackCaptures = value;}
		}
		
		public int WhiteCaptures
		{
			get {return whiteCaptures;}
			set {whiteCaptures = value;}
		}
	}
	
	public class IGSGameListener
	{
		private IGSClient client;
		private List<ObservedGame> games;
		private IGSServerInfo serverInfo;

		protected List<int> ExpectedGames {get; set;}
		
		public event IGSGameEventHandler GameChanged;
		public event IGSGameEventHandler GameAdded;
		public event IGSGameEventHandler GameEnded;	
		
		public IGSClient Client
		{
			get {return client;}
		}
		
		public IGSServerInfo ServerInfo
		{
			get {return serverInfo;}
		}
		
		public List<ObservedGame> Games
		{
			get {return games;}
		}
		
		public IGSGameListener(IGSClient client, IGSServerInfo serverInfo)
		{
			if ((client == null) ||
			    (serverInfo == null))
			{
				throw new ArgumentException("Arguments cannot be null");
			}
			
			this.client = client;
			this.serverInfo = serverInfo;
			client.AddHandler(IGSMessages.GameMove, new IGSMessageHandler(ReadMoves));						
			client.AddHandler(IGSMessages.GameStatus, new IGSMessageHandler(ReadStatus));
			client.Disconnected += new EventHandler(OnDisconnect);
			
			games = new List<ObservedGame>();
		}
				
		protected virtual void ReadMoves(List<string> lines)
		{	
			ObservedGame game = null;
			
			var header = lines[0];			
			var headerData = header.Split(' ');
			if (headerData.Length < 2)
			{
				throw new IGSParseException("Move header is corrupted: " + header);
			}
			var gameNumber = 0;
			try
			{
				gameNumber = Convert.ToInt32(headerData[1]);			
			}
			catch (FormatException)
			{
				throw new IGSParseException("Move header is corrupted: " + header);
			}
			
			foreach (var currentGame in games)
			{								
				if (gameNumber == currentGame.GameNumber)
				{
					game = currentGame;															
				}
			}
			
			if (game == null)
			{
				serverInfo.RequestGameInfo(gameNumber, new EventHandler(GameInfoReceived));	
			}
			else
			{				
				var leftBraceIndex = header.IndexOf('(');
				var lastLeftBraceIndex = header.LastIndexOf('(');
				var rightBraceIndex = header.IndexOf(')');
				var lastRightBraceIndex = header.LastIndexOf(')');
				
				if ((leftBraceIndex < 0) || 
				    (rightBraceIndex < 0))
				{
					throw new IGSParseException("Move header is corrupted: " + header);
				}
				
				var blackString = header.Substring(lastLeftBraceIndex + 1, lastRightBraceIndex - lastLeftBraceIndex - 1);
				var whiteString = header.Substring(leftBraceIndex + 1, rightBraceIndex - leftBraceIndex - 1);
				
				var blackData = blackString.Split(' ');
				var whiteData = whiteString.Split(' ');
				
				try 
				{
					game.BlackCaptures = Convert.ToInt32(blackData[0]);
					game.BlackTime = TimeSpan.FromSeconds(Convert.ToInt32(blackData[1]));
					game.BlackByouyomiStones = Convert.ToInt32(blackData[2]);
				
					game.WhiteCaptures = Convert.ToInt32(whiteData[0]);
					game.WhiteTime =TimeSpan.FromSeconds(Convert.ToInt32(whiteData[1]));
					game.WhiteByouyomiStones = Convert.ToInt32(whiteData[2]);						
				}
				catch(FormatException)
				{
					throw new IGSParseException("Move header is corrupted: " + header);
				}
				catch(IndexOutOfRangeException)
				{
					throw new IGSParseException("Move header is corrupted: " + header);
				}

				for (var i = 1; i < lines.Count; i++)
				{		
					var line = lines[i];
					var moveNumber = 0;
					try
					{
						moveNumber = Convert.ToInt32(line.Substring(0, 3));
					}
					catch (FormatException)
					{
						continue;
						//throw new IGSParseException("Move is corrupted: " + line);
					}
					if (moveNumber == game.MovesMade)
					{
						var stone = new Stone();
						stone.IsBlack = false;
						try
						{
							if (line[4] == 'B')
							{
								stone.IsBlack = true;
							}										
						}
						catch (IndexOutOfRangeException)
						{
							throw new IGSParseException("Move is corrupted: " + line);
						}
				
						var separatorIndex = line.IndexOf(':');
						if (separatorIndex < 0)
						{
							throw new IGSParseException("Move is corrupted: " + line);
						}
						
						var moveInfo = line.Substring(separatorIndex + 2).Split(' ');						
						
						if ((moveInfo.Length < 1) ||
						    (moveInfo[0].Length < 2))
						{
							throw new IGSParseException("Move is corrupted: " + line);
						}
						
						if (moveInfo[0] == "Handicap")
						{
							try
							{
								game.SetHandicap(Convert.ToByte(moveInfo[1]));
							}
							catch (FormatException)
							{
								throw new IGSParseException("Move is corrupted: " + line);						
							}
							catch (IndexOutOfRangeException)
							{
								throw new IGSParseException("Move is corrupted: " + line);						
							}
							
							game.MoveMade();
						}							
						else
						{
							var currentMove = game.MoveNumber;
							if (moveInfo[0] == "Pass")
							{
								stone.X = 20;
								stone.Y = 20;
								game.UpdateBoard = false;
								game.ToMove(game.MovesMade);
								game.PlaceStone(stone);								
								game.UpdateBoard = true;
								game.MoveMade();
							}
							else
							{	
								if (moveInfo[0][0] < 'I')
								{	
									stone.X = Convert.ToByte(moveInfo[0][0] - 'A');
								}
								else
								{
									stone.X = Convert.ToByte(moveInfo[0][0] - 'A' - 1);
								}
								stone.Y = Convert.ToByte(game.BoardSize - Convert.ToInt32(moveInfo[0].Substring(1)));
									
								game.UpdateBoard = false;
								game.ToMove(game.MovesMade);
								game.PlaceStone(stone, game.MovesMade % ConfigManager.BoardStorageStep == 0);
								game.UpdateBoard = true;
								game.MoveMade();
							}		
							if (game.MovesMade > currentMove + 1)
							{
								game.UpdateBoard = false;
								game.ToMove(currentMove);
								game.UpdateBoard = true;
							}			
							else
							{
								game.ToMove(game.MovesMade);
							}
						}
					}						
				}
				OnGameChanged(new IGSGameEventArgs(game));
			}								
		}
						
		private void GameInfoReceived(object sender, EventArgs args)
		{
			var gameInfo = (sender as IGSGameInfoRequest).Result;
			if (!games.Exists(game => game.GameNumber == gameInfo.GameNumber) &&
			    (ExpectedGames == null || 
			     ExpectedGames.Contains(gameInfo.GameNumber)))
			{
				var game = new ObservedGame(gameInfo.BoardSize, gameInfo.GameNumber);
				game.Info.BlackPlayer = gameInfo.BlackPlayer;
				game.Info.WhitePlayer = gameInfo.WhitePlayer;			
				games.Add(game);
				OnGameAdded(new IGSGameEventArgs(game));			
				client.WriteLine("moves " + Convert.ToString(gameInfo.GameNumber));			
				if (ExpectedGames != null)
				{
					ExpectedGames.Remove(gameInfo.GameNumber);
				}
			}
		}
		
		protected void OnGameChanged(IGSGameEventArgs args)
		{
			if (GameChanged != null)
			{
				GameChanged(this, args);
			}
		}
		
		protected void OnGameAdded(IGSGameEventArgs args)
		{
			if (GameAdded != null)
			{
				GameAdded(this, args);
			}
		}
		
		protected void OnGameEnded(IGSGameEventArgs args)
		{
			if (GameEnded != null)
			{
				GameEnded(this, args);
			}
		}
		
		public void ReadStatus(List<string> lines)
		{
			if (lines.Count < 2)
			{				
				throw new IGSParseException("Game status header is corrupted");													
			}
			var blackSeparatorIndex = lines[1].IndexOf(' ');
			var whiteSeparatorIndex = lines[0].IndexOf(' ');
			if ((blackSeparatorIndex < 0) ||
			    (whiteSeparatorIndex < 0))
			{
				throw new IGSParseException("Game status header is corrupted");	
			}
			
			var blackPlayer = lines[1].Substring(0, blackSeparatorIndex);
			var whitePlayer = lines[0].Substring(0, whiteSeparatorIndex);			
			ObservedGame matchGame = null;			
			foreach (ObservedGame game in Games)
			{				
				if ((game.Info.BlackPlayer == blackPlayer) ||
				    (game.Info.WhitePlayer == whitePlayer))
				{
					matchGame = game;		
				}
			}
			
			if (matchGame != null)
			{				
				if (matchGame.BoardSize != lines.Count - 2)
				{
					throw new IGSParseException("Game status data is corrupted: Board sizes don't match");	
				}
				matchGame.ToMove(matchGame.MovesMade);
				matchGame.CurrentNode.EnsureMarkup();
				matchGame.CurrentNode.Markup.EnsureTerritory();				
				var markup = matchGame.CurrentNode.Markup;
				var x = (byte) 0;
				var y = (byte) 0;
				for (var i = 2; i < lines.Count; i++)
				{
					y = 0;
					var data = lines[i].Substring(lines[i].LastIndexOf(' ') + 1);
					if (matchGame.BoardSize != data.Length)
					{
						throw new IGSParseException("Game status data is corrupted: Board sizes don't match");	
					}
					foreach (var item in data)
					{						
						switch (item - '0')
						{
							case IGSGameStatus.BlackTerritory:								
								markup.Territory.Black[x, y] = true;
								break;
								
							case IGSGameStatus.WhiteTerritory:								
								markup.Territory.White[x, y] = true;
								break;
						}
						y += 1;						
					}
					x += 1;					
				}
			
				OnGameChanged(new IGSGameEventArgs(matchGame));
			}
		}
	
		private void OnDisconnect(object sender, EventArgs args)
		{
			foreach (var game in Games)
			{
				OnGameEnded(new IGSGameEventArgs(game));
			}
			Games.Clear();
		}
	}
}

