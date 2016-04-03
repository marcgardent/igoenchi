namespace IGOEnchi.GoGameLogic
{
    public struct Mark
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly MarkType MarkType;

        public Mark(byte x, byte y, MarkType markType)
        {
            X = x;
            Y = y;
            MarkType = markType;
        }
    }
}