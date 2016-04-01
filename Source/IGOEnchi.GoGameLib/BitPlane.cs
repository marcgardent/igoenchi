using System.Collections;

namespace IGOEnchi.GoGameLogic.Models
{
    public class BitPlane
    {
        private BitArray bitArray;

        public BitPlane(byte width, byte height)
        {
            Width = width;
            Height = height;
            bitArray = new BitArray(width*height);
        }

        public BitPlane(byte size) : this(size, size)
        {
        }

        public byte Width { get; private set; }

        public byte Height { get; private set; }

        public bool this[byte xIndex, byte yIndex]
        {
            get { return bitArray.Get(yIndex*Width + xIndex); }

            set { bitArray.Set(yIndex*Width + xIndex, value); }
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

        public bool Empty()
        {
            for (byte i = 0; i < Width; i++)
            {
                for (byte j = 0; j < Height; j++)
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
    }
}