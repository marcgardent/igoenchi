using System;

namespace IGoEnchi
{
    public class IGSGameInfoRequest
    {
        private readonly EventHandler callback;

        public IGSGameInfoRequest(int gameNumber, EventHandler callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("EventHandler argument cannot be null");
            }
            GameNumber = gameNumber;
            this.callback = callback;
        }

        public int GameNumber { get; private set; }

        public IGSGameInfo Result { get; private set; }

        public void RequestCompleted(IGSGameInfo result)
        {
            Result = result;
            callback(this, new EventArgs());
        }
    }
}