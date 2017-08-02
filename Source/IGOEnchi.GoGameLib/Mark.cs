namespace IGOEnchi.GoGameLogic
{
    public struct Mark
    {
        public readonly int X;
        public readonly int Y;
        public readonly MarkType MarkType;

        public Mark(int x, int y, MarkType markType)
        {
            X = x;
            Y = y;
            MarkType = markType;
        }
    }
}