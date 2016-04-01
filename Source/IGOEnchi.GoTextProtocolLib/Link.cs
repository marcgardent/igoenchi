namespace IGoEnchi
{
    public abstract class Link
    {
        public abstract void SendCommand(string command);
        public abstract string TryReceiveCommand();
        public abstract void Close();
    }
}