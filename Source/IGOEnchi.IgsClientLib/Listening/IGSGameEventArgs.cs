using System;

namespace IGoEnchi
{
    public class IGSGameEventArgs : EventArgs
    {
        public IGSGameEventArgs(ObservedGame game)
        {
            Game = game;
        }

        public ObservedGame Game { get; private set; }
    }
}