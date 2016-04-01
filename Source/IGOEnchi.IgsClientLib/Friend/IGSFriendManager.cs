using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IgsFriendManager
    {
        private readonly IGSClient client;
        private readonly List<FriendState> friendStates;

        public IgsFriendManager(IGSClient client)
        {
            this.client = client;
            client.AddHandler(IGSMessages.Friend, OnFriendReceived);

            friendStates = new List<FriendState>();
        }

        public IEnumerable<FriendState> FriendStates
        {
            get { return friendStates; }
        }

        public event Action<FriendStateChange> FriendStateChanged;
        public event Action<string> ErrorReceived;
        public event EventHandler Updated;

        private void Sort()
        {
            friendStates.Sort(
                (f1, f2) =>
                {
                    if (f1.Online && !f2.Online)
                    {
                        return -1;
                    }
                    if (f2.Online && !f1.Online)
                    {
                        return 1;
                    }
                    return f1.Name.CompareTo(f2.Name);
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
                            state.Room));
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
                                game + "</b>"));
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
                                game + "</b>"));
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
            return text == "None"
                ? new List<int>()
                : new List<string>(text.Split(',')).
                    ConvertAll(s => Convert.ToInt32(s));
        }

        private void OnFriendReceived(List<string> lines)
        {
            foreach (var line in lines)
            {
                var data = line.Split(' ');
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