using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
 
namespace IGoEnchi
{
	public class IGSObserver: IGSGameListener
	{	
		public event IGSMessageHandler KibitzReceived;
		
		public IGSObserver(IGSClient client, IGSServerInfo serverInfo): base(client, serverInfo)
		{							
			Client.AddHandler(IGSMessages.Info, new IGSMessageHandler(this.ReadInfo));						
			Client.AddHandler(IGSMessages.Undo, new IGSMessageHandler(this.ReadUndo));						
			Client.AddHandler(IGSMessages.Kibitz, new IGSMessageHandler(this.ReadKibitz));						
			
			ExpectedGames = new List<int>();
		}				
			
		public void ObserveGame(int gameNumber)
		{		
			if (!Games.Exists(game => game.GameNumber == gameNumber) &&
			    !ExpectedGames.Contains(gameNumber))
			{			
				ExpectedGames.Add(gameNumber);
				Client.WriteLine("observe " + Convert.ToString(gameNumber));										
			}
		}		
		
		protected override void ReadMoves(List<string> lines)
		{
			var regex = new Regex(@"\s" + Client.CurrentAccount.Name + @"\s");			
			if (!regex.IsMatch(lines[0]))
			{			
				base.ReadMoves(lines);
			}
		}
		
		private void ReadInfo(List<string> lines)
		{
			if (Regex.IsMatch(lines[0], "^{?Game \\d+"))
			{
				ObservedGame finishedGame = null;
				foreach (var game in Games)
				{
					if (Regex.IsMatch(lines[0], "^{?Game " + Convert.ToString(game.GameNumber)))
					{
						finishedGame = game;
						break;
					}				
				}
				if (finishedGame != null)
				{
					Games.Remove(finishedGame);					
					OnGameEnded(new IGSGameEventArgs(finishedGame));
					MessageBox.Show(lines[0],
					                "Info",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Asterisk,
					                MessageBoxDefaultButton.Button1);
				}
			}
		}
			
		private void ReadUndo(List<string> lines)
		{
			var gameNumber = 0;
			if (lines[0].StartsWith("Undo in game"))
			{
				var separatorIndex = lines[0].IndexOf(':');
				if (separatorIndex < 0)
				{
					throw new IGSParseException("Corrupted undo data: " + (lines[0] as string));
				}
				var gameData = lines[0].Substring(0, separatorIndex);				
				try
				{
					gameNumber = Convert.ToInt32(gameData.Substring(gameData.LastIndexOf(' ') + 1));
				}
				catch (FormatException)
				{
					throw new IGSParseException("Corrupted undo data: " + (lines[0] as string));
				}
				
				ObservedGame matchGame = null;
				foreach (ObservedGame game in Games)
				{
					if (game.GameNumber == gameNumber)
					{
						matchGame = game;
					}
				}
				if (matchGame != null)
				{					
					matchGame.ToPreviousMove();
					matchGame.UndoMade();
					OnGameChanged(new IGSGameEventArgs(matchGame));
				}
			}			
		}
		
		private void ReadKibitz(List<string> lines)
		{
			OnKibitzReceived(lines);
		}
		
		protected void OnKibitzReceived(List<string> lines)
		{
			if (KibitzReceived != null)
			{
				KibitzReceived(lines);
			}
		}
		
		public void Kibitz(ObservedGame game, string message)
		{
			if (Games.Contains(game))
			{
				Client.WriteLine("kibitz " + game.GameNumber + " " + message);
			}
		}
		
		public void Unobserve(ObservedGame game)
		{
			Client.WriteLine("unobserve " + game.GameNumber);
			Games.Remove(game);
		}
	}
}

