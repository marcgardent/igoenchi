using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IGOEnchi.GoGameLogic
{
    public class BitPlane
    {
        
        private BitArray bitArray;


        public BitPlane(int width, int height)
        {
            Width = width;
            Height = height;
            bitArray = new BitArray(width*height);
        }

        public BitPlane(int size) : this(size, size)
        {

        }
        
        public int Width { get; private set; }

        public int Height { get; private set; }

        public IEnumerable<ICoords> AllCoords
        {
            get
            {
                for (int x = 0; x < this.Width; x++)
                {
                    for (int y = 0; y < this.Height; y++)
                    {
                        yield return new Coords(x, y);
                    }
                }
            }
        }

        public bool this[ICoords coords]
        {
            get { return this[coords.X, coords.Y]; }

            set { this[coords.X, coords.Y] = value; }
        }

        public bool this[int xIndex, int yIndex]
        {
            get { return bitArray.Get(yIndex * Width + xIndex); }

            set { bitArray.Set(yIndex * Width + xIndex, value); }
        }

        public bool InOfRange(ICoords coords)
        {
            return (coords.X < Width && coords.Y < Height) && (coords.X >= 0 && coords.Y >= 0);
        }
        
        public BitPlane And(BitPlane bitPlane)
        {
            var result = new BitPlane(Width, Height);
            result.bitArray = bitArray.And(bitPlane.bitArray);
            return result;
        }

        public BitPlane Or(BitPlane bitPlane)
        {
            var result = new BitPlane(Width, Height);
            result.bitArray = bitArray.Or(bitPlane.bitArray);
            return result;
        }

        public BitPlane Xor(BitPlane bitPlane)
        {
            var result = new BitPlane(Width, Height);
            result.bitArray = bitArray.Xor(bitPlane.bitArray);
            return result;
        }

        public static BitPlane operator -(BitPlane leftPlane, BitPlane rightPlane)
        {
            var result = new BitPlane(leftPlane.Width, leftPlane.Height);
            for (var i = 0; i < leftPlane.bitArray.Count; i++)
            {
                if (leftPlane.bitArray[i] && !rightPlane.bitArray[i])
                {
                    result.bitArray[i] = true;
                }
                else
                {
                    result.bitArray[i] = false;
                }
            }
            return result;
        }

        /// <summary>
        /// No True Values
        /// </summary>
        /// <returns></returns>
        public bool Empty()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (this[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public BitPlane Copy()
        {
            var bitPlaneCopy = new BitPlane(Width, Height);
            bitPlaneCopy.bitArray = bitArray.Clone() as BitArray;
            return bitPlaneCopy;
        }
        
        public IEnumerable<ICoords> Unabled
        {
            get
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (this[x, y]) yield return new Coords(x, y);
                    }
                }
            }
        }

         

        /// <summary>
        /// Remove List's Style
        /// from True to False
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(ICoords item)
        {
            if (InOfRange(item) && this[item])
            {
                this[item] = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add List's Style
        /// from False to True
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Add(ICoords item)
        {
            if (InOfRange(item))
            {
                this[item] = true;
                return true;
            }
            else
            {
                return false;
            }
        }


        public void AddRange(IEnumerable<ICoords> coords)
        {
            foreach (var coord in coords)
            {
                this[coord] = true;
            }
        }

        public int Count => Unabled.Count();
    }
}