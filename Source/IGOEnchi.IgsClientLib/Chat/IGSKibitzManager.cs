using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IGSKibitzManager : IGSChatManager
    {
        private readonly IGSObserver gameObserver;

        public IGSKibitzManager(IGSObserver gameObserver)
        {
            if (gameObserver == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }

            this.gameObserver = gameObserver;
            gameObserver.KibitzReceived += GetMessage;
        }

        public new event IGSChatHandler DefaultCallback;

        private void GetMessage(List<string> lines)
        {
            var beginningIndex = lines[0].IndexOf(' ');
            var separatorIndex = lines[0].IndexOf(':');
            var leftBraceIndex = lines[0].LastIndexOf('[');
            var rightBraceIndex = lines[0].LastIndexOf(']');

            if ((separatorIndex < 0) || (separatorIndex < 0) ||
                (leftBraceIndex < 0) || (rightBraceIndex < 0))
            {
                throw new IGSParseException("Syntax error in KIBITZ message: " + lines[0]);
            }

            var name = lines[0].Substring(beginningIndex + 1, separatorIndex - beginningIndex - 1);
            var gameNumber =
                Convert.ToInt32(lines[0].Substring(leftBraceIndex + 1, rightBraceIndex - leftBraceIndex - 1));
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