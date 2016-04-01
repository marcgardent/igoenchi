using System;

namespace IGoEnchi
{
    public class IGSFileReceivedEventArgs : EventArgs
    {
        public IGSFileReceivedEventArgs(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; private set; }
        public string Content { get; private set; }
    }
}