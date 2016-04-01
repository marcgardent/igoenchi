using System;
using System.Collections.Generic;

namespace IGoEnchi
{
	public delegate void IGSChatHandler(string userName, string message);
	
	public class IGSChatManager
	{		
		private Dictionary<string, IGSChatHandler> dictionary;
		
		public event IGSChatHandler DefaultCallback;
		
		public IGSChatManager()
		{
			dictionary = new Dictionary<string, IGSChatHandler>();
		}
		
		protected Dictionary<string, IGSChatHandler> Dictionary
		{
			get {return dictionary;}
		}
		
		public void RegisterChat(string name, IGSChatHandler handler)
		{
			if ((name == null) ||
			    (handler == null))
			{
				throw new ArgumentException("Cannot register handler");
			}
			if (dictionary.ContainsKey(name))
			{
				throw new ArgumentException("This handler is already registered");
			}
						
			dictionary[name] = handler;
		}
		
		public void UnregisterChat(string name)
		{
			if (dictionary.ContainsKey(name))
			{
				dictionary.Remove(name);
			}			    
		}
		
		protected void ForwardMessage(string name, string text)
		{
			if (dictionary.ContainsKey(name))
			{
				dictionary[name](name, text);
			}
			else
			{
				if (DefaultCallback != null)
				{
					DefaultCallback(name, text);
				}
			}
		}
					
		public virtual void SendMessage(string userName, string message)
		{			
		}
	}
	
	public class IGSTellManager: IGSChatManager
	{
		private IGSClient client;		
		
		public IGSTellManager(IGSClient client)
		{
			if (client == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			this.client = client;
			client.AddHandler(IGSMessages.Chat, new IGSMessageHandler(GetMessage));
		}

		private void GetMessage(List<string> lines)
		{
			var separatorIndex = lines[0].IndexOf(':');
			
			if (separatorIndex < 0)
			{
				throw new IGSParseException("Syntax error in TELL message: " + (lines[0] as string));
			}
							
			var name = lines[0].Substring(1, separatorIndex - 2);			
			var text = lines[0].Substring(separatorIndex + 1);
			
			ForwardMessage(name, text);
		}
			
		public override void SendMessage(string userName, string message)
		{
			client.WriteLine("tell " + userName + " " + message);			
			base.SendMessage(userName, message);
		}
	}
	
	public class IGSSayManager: IGSChatManager
	{
		private IGSPlayer gamePlayer;
		
		public IGSSayManager(IGSPlayer gamePlayer): base()
		{
			if (gamePlayer == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			this.gamePlayer = gamePlayer;
			gamePlayer.SayReceived += new IGSMessageHandler(GetMessage);
		}
		
		private void GetMessage(List<string> lines)
		{
			var separatorIndex = lines[0].IndexOf(':');
			
			if (separatorIndex < 0)
			{
				throw new IGSParseException("Syntax error in TELL message: " + (lines[0] as string));
			}
							
			var name = lines[0].Substring(1, separatorIndex - 2);			
			var text = lines[0].Substring(separatorIndex + 1);
			
			ForwardMessage(name, text);
		}
		
		public override void SendMessage(string userName, string message)
		{
			foreach (ObservedGame game in gamePlayer.Games)
			{
				if ((game.Info.BlackPlayer == userName) ||
				    (game.Info.WhitePlayer == userName))
				{
					gamePlayer.Say(game, message);						
				}
			}
			base.SendMessage(userName, message);
		}
	}
	
	public class IGSKibitzManager: IGSChatManager
	{
		private IGSObserver gameObserver;
		
		public new event IGSChatHandler DefaultCallback;
		
		public IGSKibitzManager(IGSObserver gameObserver): base()
		{
			if (gameObserver == null)
			{
				throw new ArgumentException("Argument cannot be null");
			}
			
			this.gameObserver = gameObserver;
			gameObserver.KibitzReceived += new IGSMessageHandler(GetMessage);
		}
		
		private void GetMessage(List<string> lines)
		{
			var beginningIndex = lines[0].IndexOf(' ');
			var separatorIndex = lines[0].IndexOf(':');
			var leftBraceIndex = lines[0].LastIndexOf('[');
			var rightBraceIndex = lines[0].LastIndexOf(']');
			
			if ((separatorIndex < 0) || (separatorIndex < 0) ||
			    (leftBraceIndex < 0) || (rightBraceIndex < 0))
			{
				throw new IGSParseException("Syntax error in KIBITZ message: " + (lines[0] as string));
			}
							
			var name = lines[0].Substring(beginningIndex + 1, separatorIndex - beginningIndex - 1);
			var gameNumber = Convert.ToInt32(lines[0].Substring(leftBraceIndex + 1, rightBraceIndex - leftBraceIndex - 1));
			var text = lines[1];
			
			ForwardMessage(gameNumber.ToString(), name, text);
		}
		
		protected void ForwardMessage(string game, string name, string text)
		{
			if (Dictionary.ContainsKey(game))
			{
				Dictionary[game](name, text);
			}
			else
			{
				if (DefaultCallback != null)
				{
					DefaultCallback(game, text);
					if (Dictionary.ContainsKey(game))
					{
						Dictionary[game](name, text);
					}
				}
			}
		}
				
		public override void SendMessage(string userName, string message)
		{
			foreach (var game in gameObserver.Games)
			{
				if (game.GameNumber.ToString() == userName)
				{
					gameObserver.Kibitz(game, message);
				}
			}
			base.SendMessage(userName, message);
		}
	}
}
