using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IGSSayManager : IGSChatManager
    {
        private readonly IGSPlayer gamePlayer;

        public IGSSayManager(IGSPlayer gamePlayer)
        {
            if (gamePlayer == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }

            this.gamePlayer = gamePlayer;
            gamePlayer.SayReceived += GetMessage;
        }

        private void GetMessage(List<string> lines)
        {
            var separatorIndex = lines[0].IndexOf(':');

            if (separatorIndex < 0)
            {
                throw new IGSParseException("Syntax error in TELL message: " + lines[0]);
            }

            var name = lines[0].Substring(1, separatorIndex - 2);
            var text = lines[0].Substring(separatorIndex + 1);

            ForwardMessage(name, text);
        }

        public override void SendMessage(string userName, string message)
        {
            foreach (var game in gamePlayer.Games)
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
}