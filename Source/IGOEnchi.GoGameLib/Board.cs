using System;
using System.Collections.Generic;

namespace IGOEnchi.GoGameLogic
{
    public class Board
    {
        private Board()
        {

        }

        public Board(byte boardSize)
        {
            Black = new BitPlane(boardSize);
            White = new BitPlane(boardSize);
        }

        public BitPlane Black { get; private set; }

        public BitPlane White { get; private set; }

        public BitPlane BlackAndWhite => White.Or(Black);

        public IEnumerable<ICoords> AllCoords
        {
            get
            {
                for (byte x = 0; x < this.Size; x++)
                {
                    for (byte y = 0; y < this.Size; y++)
                    {
                        yield return new Coords(x,y);
                    }
                }
            }
        }

        public byte Size => Black.Width;

        public Board Copy()
        {
            var result = new Board();
            result.Black = Black.Copy();
            result.White = White.Copy();
            return result;
        }

        public bool Place(byte x, byte y, bool black)
        {
            if (StoneInBounds(x, y))
            {
                if (black)
                {
                    Black[x, y] = true;
                }
                else
                {
                    White[x, y] = true;
                }
                return true;
            }
            return false;
        }

        public bool Place(Stone stone, bool killGroup)
        {
            if (Place(stone.X, stone.Y, stone.IsBlack))
            {
                if (killGroup)
                {
                    KillGroup(stone);
                }
                return true;
            }
            return false;
        }

        public bool Place(Stone stone)
        {
            return Place(stone, true);
        }

        public bool Remove(byte x, byte y)
        {
            Black[x, y] = false;
            White[x, y] = false;
            return true;
        }

        public bool Remove(ICoords stone)
        {
            return Remove(stone.X, stone.Y);
        }


        
        public bool HasBlack(byte x, byte y)
        {
            return StoneInBounds(x, y) && Black[x, y];
        }

        public bool HasWhite(byte x, byte y)
        {
            return StoneInBounds(x, y) && White[x, y];
        }

        [Obsolete]
        public bool HasStone(byte x, byte y, bool black)
        {
            if (StoneInBounds(x, y))
            {
                if (black)
                {
                    return Black[x, y];
                }
                return White[x, y];
            }
            return false;
        }

        public bool HasStone(Stone stone)
        {
            if (StoneInBounds(stone))
            {
                if (stone.IsBlack)
                {
                    return Black[stone.X, stone.Y];
                }
                return White[stone.X, stone.Y];
            }
            return false;
        }


        /// <summary>
        /// Is coords on the board
        /// </summary>
        public bool StoneInBounds(byte x, byte y)
        {
            return (x < Black.Width) && (y < Black.Height);
        }

        /// <summary>
        /// Is coords on the board
        /// </summary>
        public bool StoneInBounds(ICoords stone)
        {
            return StoneInBounds(stone.X, stone.Y);
        }

        public bool GroupIsDead(Stone stone)
        {
            if (HasStone(stone))
            {
                var bitPlane = new BitPlane(Black.Width,
                    Black.Height);

                var stonesToCheck = new List<Stone>();
                stonesToCheck.Add(stone);

                while (stonesToCheck.Count > 0)
                {
                    var thisStone = stonesToCheck[0];
                    if (!bitPlane[thisStone.X, thisStone.Y])
                    {
                        bitPlane[thisStone.X, thisStone.Y] = true;

                        var nextStone = thisStone.TopStone();
                        if (HasStone(nextStone))
                        {
                            stonesToCheck.Add(nextStone);
                        }
                        else
                        {
                            if (StoneInBounds(nextStone.OtherColorStone()) &&
                                !HasStone(nextStone.OtherColorStone()))
                            {
                                return false;
                            }
                        }

                        nextStone = thisStone.LeftStone();
                        if (HasStone(nextStone))
                        {
                            stonesToCheck.Add(nextStone);
                        }
                        else
                        {
                            if (StoneInBounds(nextStone.OtherColorStone()) &&
                                !HasStone(nextStone.OtherColorStone()))
                            {
                                return false;
                            }
                        }

                        nextStone = thisStone.BottomStone();
                        if (HasStone(nextStone))
                        {
                            stonesToCheck.Add(nextStone);
                        }
                        else
                        {
                            if (StoneInBounds(nextStone.OtherColorStone()) &&
                                !HasStone(nextStone.OtherColorStone()))
                            {
                                return false;
                            }
                        }

                        nextStone = thisStone.RightStone();
                        if (HasStone(nextStone))
                        {
                            stonesToCheck.Add(nextStone);
                        }
                        else
                        {
                            if (StoneInBounds(nextStone.OtherColorStone()) &&
                                !HasStone(nextStone.OtherColorStone()))
                            {
                                return false;
                            }
                        }

                        stonesToCheck.Remove(thisStone);
                    }
                    else
                    {
                        stonesToCheck.Remove(thisStone);
                    }
                }

                return true;
            }
            return false;
        }

        private void RemoveGroup(Stone stone)
        {
            if (!HasStone(stone))
            {
                throw new Exception("Invalid group token");
            }
            var stonesToCheck = new List<Stone>();

            stonesToCheck.Add(stone);

            while (stonesToCheck.Count > 0)
            {
                var thisStone = stonesToCheck[0];

                Remove(thisStone);

                var nextStone = thisStone.TopStone();
                if (HasStone(nextStone))
                {
                    stonesToCheck.Add(nextStone);
                }

                nextStone = thisStone.LeftStone();
                if (HasStone(nextStone))
                {
                    stonesToCheck.Add(nextStone);
                }

                nextStone = thisStone.BottomStone();
                if (HasStone(nextStone))
                {
                    stonesToCheck.Add(nextStone);
                }

                nextStone = thisStone.RightStone();
                if (HasStone(nextStone))
                {
                    stonesToCheck.Add(nextStone);
                }

                stonesToCheck.Remove(thisStone);
            }
        }

        public void KillGroup(Stone stone)
        {
            if (GroupIsDead(stone.TopStone().OtherColorStone()))
            {
                RemoveGroup(stone.TopStone().OtherColorStone());
            }

            if (GroupIsDead(stone.LeftStone().OtherColorStone()))
            {
                RemoveGroup(stone.LeftStone().OtherColorStone());
            }

            if (GroupIsDead(stone.BottomStone().OtherColorStone()))
            {
                RemoveGroup(stone.BottomStone().OtherColorStone());
            }

            if (GroupIsDead(stone.RightStone().OtherColorStone()))
            {
                RemoveGroup(stone.RightStone().OtherColorStone());
            }
        }
    }
}