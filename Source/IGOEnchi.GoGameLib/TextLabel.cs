namespace IGOEnchi.GoGameLogic
{
    public struct TextLabel
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly string Text;

        public TextLabel(byte x, byte y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
    }
}