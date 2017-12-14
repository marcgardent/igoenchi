using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.Maps;

namespace IGOPhoenix.GoGameAnalytic.BitPlaneParsing
{
    public class LocalInfluence
    {
        public readonly Map localInfluence;

        public readonly BitPlane White;
        public readonly BitPlane Black;

        public LocalInfluence(Board board)
        {
             this.localInfluence = new Map(board.Size, board.Size);
            White = new BitPlane(board.Size);
            Black= new BitPlane(board.Size);

            foreach (var allCoord in board.AllCoords)
            {
                if (board.White[allCoord])
                {
                    White[allCoord] = true;
                }
                else if (board.Black[allCoord])
                {
                    Black[allCoord] = true;
                }
                else { 

                    foreach (var rank in AnalyticHelper.OilStainWalker(board.White, allCoord))
                    {
                        var whites = rank.Count(x => board.White[x]);
                        var blacks = rank.Count(x => board.Black[x]);

                        if (whites > 0 || blacks > 0)
                        {
                            if (whites>blacks)
                            {
                                this.White[allCoord] = true;
                            }
                            else if (whites < blacks)
                            {
                                this.Black[allCoord] = true;
                            }

                            break;
                        }
                    }
                }
            }
        }
    }

    public class TerritoryParser
    {
        public enum BorderState
        {
            left,
            right,
            top,
            bottom
        }

        public class TerritoryCoords : ICoords
        {
            public IEnumerable<BorderState> Orientations { get; }
            public int X { get; }
            public int Y { get; }
            
            public TerritoryCoords(byte x, byte y)
            {
                X = x;
                Y = y;
            }

            public TerritoryCoords(ICoords coords, IEnumerable<BorderState> orientations)
            {
                Orientations = orientations;
                X = coords.X;
                Y = coords.Y;
            }
        }
        
        public List<TerritoryCoords> Parse(BitPlane bitPlane)
        {
            List<TerritoryCoords> ret  = new List<TerritoryCoords>();
            foreach (var coord in bitPlane.Unabled )
            {
                var limits = new List<BorderState>();
                
                var left = new Coords(coord.X - 1, coord.Y);
                var right = new Coords(coord.X + 1, coord.Y);
                var top = new Coords(coord.X , coord.Y-1);
                var bottom = new Coords(coord.X , coord.Y+1);
                /*
                if (!bitPlane.InOfRange(left) || !bitPlane[left]) limits.Add(BorderState.left); 
                if (!bitPlane.InOfRange(right) || !bitPlane[right]) limits.Add(BorderState.right); 
                if (!bitPlane.InOfRange(top) || !bitPlane[top]) limits.Add(BorderState.top); 
                if (!bitPlane.InOfRange(bottom) || !bitPlane[bottom]) limits.Add(BorderState.bottom);
                */

                if (bitPlane.InOfRange(left) &&!bitPlane[left]) limits.Add(BorderState.left);
                if (bitPlane.InOfRange(right) && !bitPlane[right]) limits.Add(BorderState.right);
                if (bitPlane.InOfRange(top) && !bitPlane[top]) limits.Add(BorderState.top);
                if (bitPlane.InOfRange(bottom) && !bitPlane[bottom]) limits.Add(BorderState.bottom);

                ret.Add(new TerritoryCoords(coord, limits));
            }

            return ret;
        }
    }
}