using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IGSTellManager : IGSChatManager
    {
        private readonly IGSClient client;

        public IGSTellManager(IGSClient client)
        {
            if (client == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }

            this.client = client;
            client.AddHandler(IGSMessages.Chat, GetMessage);
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
            client.WriteLine("tell " + userName + " " + message);
            base.SendMessage(userName, message);
        }
    }
}