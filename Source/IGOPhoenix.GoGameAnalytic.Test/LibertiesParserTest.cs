using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{


    [TestClass]
    public class RayTest
    {

        [TestMethod]
        public void When_Move_To_HortogonalTop()
        {
            var target = new Ray(new Coords(10,10), new Coords(10, 5));
            var actual = target.Points.ToArray();
             
           AssertCoords(new[]
           { 
            new Coords(10, 9),
            new Coords(10, 8),
            new Coords(10, 7),
            new Coords(10, 6),
            new Coords(10, 5),
            }, actual);
        }

        [TestMethod]
        public void When_Move_To_HortogonalBottom()
        {
            var target = new Ray(new Coords(10, 10), new Coords(10, 15));
            var actual = target.Points.ToArray();


            AssertCoords(new[]
           {
                new Coords(10, 11),
                new Coords(10, 12),
                new Coords(10, 13),
                new Coords(10, 14),
                new Coords(10, 15),
            }, actual);
        }


        [TestMethod]
        public void When_Move_To_HortogonalLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 10));
            var actual = target.Points.ToArray();

            AssertCoords(new[]
           {
                new Coords(9, 10),
                new Coords(8, 10),
                new Coords(7, 10),
                new Coords(6, 10),
                new Coords(5, 10),
            }, actual);
             
        }

        [TestMethod]
        public void When_Move_To_HortogonalRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 10));
            var actual = target.Points.ToArray();
            
            AssertCoords(new[]
            {
                new Coords(11, 10),
                new Coords(12, 10),
                new Coords(13, 10),
                new Coords(14, 10),
                new Coords(15, 10),
            }, actual);
        }

        [TestMethod]
        public void When_Move_To_TopLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 5));
            var actual = target.Points.ToArray();
            AssertCoords(new[]
            {
                new Coords(9, 9),
                new Coords(8, 8),
                new Coords(7, 7),
                new Coords(6, 6),
                new Coords(5, 5),
            }, actual);
             
        }

        [TestMethod]
        public void When_Move_To_TopRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 5));
            var actual = target.Points.ToArray();


            AssertCoords(new[]
            {
                new Coords(11, 9),
                new Coords(12, 8),
                new Coords(13, 7),
                new Coords(14, 6),
                new Coords(15, 5),
            }, actual);
            
        }

        [TestMethod]
        public void When_Move_To_BottomRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 15));
            var actual = target.Points.ToArray();

            AssertCoords(new[]
            {
                new Coords(11, 11),
                new Coords(12, 12),
                new Coords(13, 13),
                new Coords(14, 14),
                new Coords(15, 15),
            }, actual);

        }

        [TestMethod]
        public void When_Move_To_BottomLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 15));
            var actual = target.Points.ToArray();
            
            AssertCoords(new []
            {
                new Coords(9, 11),
                new Coords(8, 12),
                new Coords(7, 13),
                new Coords(6, 14),
                new Coords(5, 15)
            }, actual);
        }

        private static void AssertCoords(ICoords[] excepted, ICoords[] actual)
        {
            Trace.WriteLine("excepted" + String.Join(" ",excepted.Select(x=> $"({x.X},{x.Y})")));
            Trace.WriteLine("actual" + String.Join(" ", actual.Select(x=> $"({x.X},{x.Y})")));

            Assert.AreEqual(excepted.Length, actual.Length);

            for (int i = 0; i < excepted.Length; i++)
            {
                Assert.AreEqual(excepted[i].X, actual[i].X);
                Assert.AreEqual(excepted[i].Y, actual[i].Y);
            }
        }
    }


    [TestClass]
    public class LibertiesParserTest
    {
  
        [TestMethod]
        public void When_Tengen_Do_Four()
        {
            var target = new LibertiesParser();

            var data = new BitPlane(11);
            data.Add(new Coords(5, 5)); 

            var actual = target.Parse(data, data);
            var excepted = new BitPlane(11);
            excepted.Add(new Coords(4, 5));
            excepted.Add(new Coords(6, 5));
            excepted.Add(new Coords(5, 4));
            excepted.Add(new Coords(5, 6));

            BitPlaneTestHelper.AssertBitPlane(excepted, actual, "Tengen");
        }

        [TestMethod]
        public void When_Corner_Do_Two()
        {
            var target = new LibertiesParser();

            var data = new BitPlane(11);
            data.Add(new Coords(0, 0));

            var actual = target.Parse(data, data);
            var excepted = new BitPlane(11);
            excepted.Add(new Coords(0, 1));
            excepted.Add(new Coords(1, 0));
            
            BitPlaneTestHelper.AssertBitPlane(excepted, actual, "Corner");
        }
        
        [TestMethod]
        public void When_Atari_Do_Zero()
        {
            var target = new LibertiesParser();

            var grp = new BitPlane(11);
            grp.Add(new Coords(5, 5));
            
            var all = new BitPlane(11);
            all.Add(new Coords(5, 5));
            all.Add(new Coords(4, 5));
            all.Add(new Coords(6, 5));
            all.Add(new Coords(5, 4));
            all.Add(new Coords(5, 6));

            var actual = target.Parse(grp, all);
            var excepted = new BitPlane(11);
            BitPlaneTestHelper.AssertBitPlane(excepted, actual, "Atari");
        }
    }
}
