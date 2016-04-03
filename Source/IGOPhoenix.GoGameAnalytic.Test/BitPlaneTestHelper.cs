using System.Diagnostics;
using IGOEnchi.GoGameLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
    public static class BitPlaneTestHelper
    {
        public static void AssertBitPlane(BitPlane excepted, BitPlane actual, string message)
        {
            var compare = excepted.Copy();
            compare.Xor(actual);

            if (!compare.Empty())
            {
                Trace.WriteLine(message);
                TraceBitPlane(actual, "actual :");
                TraceBitPlane(excepted, "excepted :");
                TraceBitPlane(compare, "Errors :");
                Assert.Fail("See Trace");
            }
            else
            {
                TraceBitPlane(actual,"Ok!");
            }
        }

        public static void TraceBitPlane(BitPlane plane, string name)
        {
            Trace.WriteLine(name);
            for (byte i = 0; i < plane.Width; i++)
            {
                for (byte j = 0; j < plane.Height; j++)
                {
                    Trace.Write(plane[i, j] ? "X" : "O");
                }
                Trace.WriteLine($"|{i}");
            }
        }
    }
}