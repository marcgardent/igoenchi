namespace IGOEnchi.GoGameLogic {
    public class Coords : ICoords
    {
        
        public byte X { get; private set; }
        public byte Y { get; private set; }

        public Coords(byte x, byte y)
        {
            X = x;
            Y = y;
        }

    }
}