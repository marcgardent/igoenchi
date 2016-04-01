using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public class IGSChatManager
    {
        public IGSChatManager()
        {
            Dictionary = new Dictionary<string, IGSChatHandler>();
        }

        protected Dictionary<string, IGSChatHandler> Dictionary { get; private set; }

        public event IGSChatHandler DefaultCallback;

        public void RegisterChat(string name, IGSChatHandler handler)
        {
            if ((name == null) ||
                (handler == null))
            {
                throw new ArgumentException("Cannot register handler");
            }
            if (Dictionary.ContainsKey(name))
            {
                throw new ArgumentException("This handler is already registered");
            }

            Dictionary[name] = handler;
        }

        public void UnregisterChat(string name)
        {
            if (Dictionary.ContainsKey(name))
            {
                Dictionary.Remove(name);
            }
        }

        protected void ForwardMessage(string name, string text)
        {
            if (Dictionary.ContainsKey(name))
            {
                Dictionary[name](name, text);
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
}