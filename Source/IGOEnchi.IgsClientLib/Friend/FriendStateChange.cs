namespace IGoEnchi
{
    public class FriendStateChange
    {
        public FriendStateChange(string message)
        {
            Message = message;
            GameNumber = Maybe<int>.None;
        }

        public FriendStateChange(string message, int game)
        {
            Message = message;
            GameNumber = Maybe.Some(game);
        }

        public string Message { get; private set; }
        public Maybe<int> GameNumber { get; private set; }
    }
}