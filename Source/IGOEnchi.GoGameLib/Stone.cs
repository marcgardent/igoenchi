using System;

namespace IGOEnchi.GoGameLogic
{
    public class Stone : ICoords
    {
        public int X { set; get; }

        public int Y { set; get; }

        public bool IsBlack { set; get; }
        public bool IsWhite => !IsBlack;

        public Stone()
        {
            X = 0;
            Y = 0;
            IsBlack = true;
        }

        public Stone(int x, int y, bool isBlack)
        {
            X = x;
            Y = y;
            IsBlack = isBlack;
        }

        public Stone(Stone source)
        {
            X = source.X;
            Y = source.Y;
            IsBlack = source.IsBlack;
        }


        public Stone OtherColorStone()
        {
            var stone = new Stone(this);
            stone.IsBlack = !IsBlack;

            return stone;
        }

        public bool AtPlaceOf(Stone other)
        {
            return X == other.X && Y == other.Y;
        }

        public Stone TopStone()
        {
            var stone = new Stone(this);
            if (Y > 0)
            {
                stone.Y = stone.Y - 1;
            }
            else
            {
                stone.Y = int.MaxValue;
            }

            return stone;
        }

        public Stone LeftStone()
        {
            var stone = new Stone(this);
            if (X > 0)
            {
                stone.X = stone.X - 1;
            }
            else
            {
                stone.X = int.MaxValue;
            }

            return stone;
        }

        public Stone BottomStone()
        {
            var stone = new Stone(this);
            stone.Y = stone.Y + 1;

            return stone;
        }

        public Stone RightStone()
        {
            var stone = new Stone(this);
            stone.X = stone.X + 1;

            return stone;
        }

        public bool SameAs(Stone stone)
        {
            return X == stone.X && Y == stone.Y && IsBlack == stone.IsBlack;
        }
    }
}