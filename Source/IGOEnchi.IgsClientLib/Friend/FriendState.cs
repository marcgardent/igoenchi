using System.Collections.Generic;

namespace IGoEnchi
{
    public class FriendState
    {
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

        public string Name { get; private set; }
        public bool Online { get; private set; }
        public int Room { get; private set; }
        public List<int> PlayedGames { get; private set; }
        public List<int> ObservedGames { get; private set; }

        public override string ToString()
        {
            var name = Name + (Online ? "" : "(offline)");
            var playing =
                PlayedGames.Count > 0
                    ? ", playing in game " + PlayedGames[0]
                    : "";
            var observing =
                ObservedGames.Count > 0 && playing == ""
                    ? ", observing game " + ObservedGames[0]
                    : "";

            return name + playing + observing;
        }
    }
}