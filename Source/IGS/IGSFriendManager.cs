using System;
using System.Collections.Generic;
 
namespace IGoEnchi
{
	public class FriendState
	{
		public string Name {get; private set;}
		public bool Online {get; private set;}
		public int Room {get; private set;}
		public List<int> PlayedGames {get; private set;}
		public List<int> ObservedGames {get; private set;}		
		
		public FriendState(string name, bool online, int room,
		                   List<int> playedGames,
		                   List<int> observedGames)
		{
			Name = name;
			Online = online;
			Room = room;
			PlayedGames = playedGames;
			ObservedGames = observedGames;
		}
		
		public override string ToString()
		{
			var name = Name + (Online ? "" : ("(offline)"));
			var playing =
				PlayedGames.Count > 0 ?
					", playing in game " + PlayedGames[0].ToString() :
					"";
			var observing = 
				(ObservedGames.Count > 0 && playing == "") ?
					", observing game " + ObservedGames[0].ToString() :
					"";
				
			return name + playing + observing;
		}
	}
	
	public class FriendStateChange
	{
		public string Message {get; private set;}
		public Maybe<int> GameNumber {get; private set;}
		
		public FriendStateChange(string message)
		{
			Message = message;
			GameNumber = Maybe<int>.None;
		}
		
		public FriendStateChange(string message, int game)
		{
			Message = message;
			GameNumber = Maybe.Some(game);
		}
	}
	
	public class IGSFriendManager
	{
		private IGSClient client;
		private List<FriendState> friendStates;
		
		public event Action<FriendStateChange> FriendStateChanged;
		public event Action<string> ErrorReceived;
		public event EventHandler Updated;
		
		public IGSFriendManager(IGSClient client)
		{
			this.client = client;			
			client.AddHandler(IGSMessages.Friend, OnFriendReceived);
			
			friendStates = new List<FriendState>();			
		}			

		public IEnumerable<FriendState> FriendStates
		{
			get {return friendStates;}
		}
		
		private void Sort()
		{
			friendStates.Sort(
				(f1, f2) =>
				{
					if (f1.Online && !f2.Online)
					{
						return -1;
					}
					else if (f2.Online && !f1.Online)
					{
						return 1;
					}
					else
					{
						return f1.Name.CompareTo(f2.Name);
					}
				});
		}
		
		private void ChangeState(FriendState state)
		{
			var changed = false;
			var index = friendStates.FindIndex(s => s.Name == state.Name);
			if (index >= 0)
			{
				var lastState = friendStates[index];				
				if (state.Online != lastState.Online)
				{
					OnFriendStateChanged(
						new FriendStateChange(
							state.Name + " has logged " +
								(state.Online ? "on" : "off")));					
					changed = true;
				}
				else if (state.Room != lastState.Room)
				{
					OnFriendStateChanged(
						new FriendStateChange(
							state.Name + " has moved to room " + 
								state.Room.ToString()));
					changed = true;
				}			
				else
				{
					var addedGames = new List<int>(state.ObservedGames);
						addedGames.RemoveAll(
							game => lastState.ObservedGames.Contains(game));
					var removedGames = new List<int>(lastState.ObservedGames);
						removedGames.RemoveAll(
							game => state.ObservedGames.Contains(game));
					
					foreach (var game in addedGames)
					{
						OnFriendStateChanged(
							new FriendStateChange(
								state.Name + " is observing ", game));
						changed = true;
					}
					foreach (var game in removedGames)
					{
						OnFriendStateChanged(
							new FriendStateChange(
								state.Name + " has stopped observing <b>Game " + 
								game.ToString() + "</b>"));
						changed = true;
					}
					
					addedGames = new List<int>(state.PlayedGames);
						addedGames.RemoveAll(
							game => lastState.PlayedGames.Contains(game));
					removedGames = new List<int>(lastState.PlayedGames);
						removedGames.RemoveAll(
							game => state.PlayedGames.Contains(game));
					
					foreach (var game in addedGames)
					{
						OnFriendStateChanged(
							new FriendStateChange(
								state.Name + " is playing in ", game));
						changed = true;
					}
					foreach (var game in removedGames)
					{
						OnFriendStateChanged(
							new FriendStateChange(
								state.Name + " has stopped playing in <b>Game " +
								game.ToString() + "</b>"));
						changed = true;
					}
				}
				
				friendStates[index] = state;
				
				if (changed)
				{
					Sort();
					OnUpdated();					
				}
			}
			else
			{				
				friendStates.Add(state);
				Sort();
				OnUpdated();
			}
		}
		
		private void OnFriendStateChanged(FriendStateChange change)
		{
			if (FriendStateChanged != null)
			{
				FriendStateChanged(change);
			}
		}
		
		private void OnUpdated()
		{
			if (Updated != null)
			{				
				Updated(this, EventArgs.Empty);
			}
		}
		
		private List<int> ParseGames(string text)
		{
			return text == "None" ? new List<int>() :
				new List<string>(text.Split(new char[] {','})).
					ConvertAll(s => Convert.ToInt32(s));
		}
		
		private void OnFriendReceived(List<string> lines)
		{
			foreach (var line in lines)
			{
				var data = line.Split(new char[] {' '});
				if (data[0] == "ERROR:")
				{
					OnErrorReceived(line.Remove(0, "ERROR:".Length));
				}				
				else if (!data[0].EndsWith(":"))
				{					
					var name = data[0];
					var online = data[2] != "0";
					var room = Convert.ToInt32(data[3]);
					
					var playedGames = ParseGames(data[4]);
					var observedGames = ParseGames(data[5]);						
					ChangeState(
						new FriendState(name, online, room, 
						                playedGames, observedGames));
				}
			}
		}								

		private void OnErrorReceived(string error)
		{
			if (ErrorReceived != null)
			{
				ErrorReceived(error);
			}
		}
		
		public void Update()
		{
			client.WriteLine("friend list");
		}
		
		private void Command(string command, string name)
		{
			client.WriteLine("friend " + command + " " + name);
		}				
		
		public void Add(string name)
		{
			Command("add", name.Trim());
		}
		
		public void Remove(string name)
		{			
			Command("del", name);
			friendStates.RemoveAll(s => s.Name == name);
			OnUpdated();
		}
		
		public void Block(string name)
		{
			Command("refuse", name);
		}
		
		public void Allow(string name)
		{
			Command("unrefuse", name);
		}				
	}
}