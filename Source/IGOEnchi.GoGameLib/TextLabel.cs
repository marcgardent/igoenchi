namespace IGOEnchi.GoGameLogic
{
    public struct TextLabel
    {
        public readonly int X;
        public readonly int Y;
        public readonly string Text;

        public TextLabel(int x, int y, string text)
        {
            X = x;
            Y = y;
            Text = text;
        }
    }
}