using System;
using System.Diagnostics;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
    [TestClass]
    public class RayTest
    {
        [TestMethod]
        public void Ray_should_be_Commutative_Operation()
        {
            var boardsize = 19;
            for (byte x = 0; x < boardsize; x++)
                for (byte y = 0; y < boardsize; y++)
                    for (byte xp = 0; xp < boardsize; xp++)
                        for (byte yp = 0; yp < boardsize; yp++)
                        {
                            var a = new RayCoords(new Coords(x, y)); var b = new RayCoords(new Coords(xp, yp));
                            var excepted = new[] { a }.Union(new Ray(a, b).Points).ToArray();
                            var actual = new[] { b }.Union(new Ray(b, a).Points).Reverse().ToArray();
                            Trace.WriteLine($"test ({x},{y})<->({xp},{yp})");
                            AssertCoords(excepted, actual);
                        }
        }

        [TestMethod]
        public void When_Move_To_HortogonalTop()
        {
            var target = new Ray(new Coords(10,10), new Coords(10, 5));
            var actual = target.Points.ToArray();
             
            AssertCoords(new[]
            { 
                new RayCoords(10, 9),
                new RayCoords(10, 8),
                new RayCoords(10, 7),
                new RayCoords(10, 6),
                new RayCoords(10, 5),
            }, actual);
        }

        [TestMethod]
        public void When_Move_To_QuasiHortogonal()
        {
            var target = new Ray(new Coords(10, 10), new Coords(16, 11));
            var actual = target.Points.ToArray();

            AssertCoords(new[]
            {
                new RayCoords(11, 10),
                new RayCoords(12, 10),
                new RayCoords(13, 10),
                new RayCoords(13, 11),
                new RayCoords(14, 11),
                new RayCoords(15, 11),
                new RayCoords(16, 11),
            }, actual);
        }

        [TestMethod]
        public void When_Move_To_HortogonalBottom()
        {
            var target = new Ray(new Coords(10, 10), new Coords(10, 15));
            var actual = target.Points.ToArray();


            AssertCoords(new[]
            {
                new RayCoords(10, 11),
                new RayCoords(10, 12),
                new RayCoords(10, 13),
                new RayCoords(10, 14),
                new RayCoords(10, 15),
            }, actual);
        }


        [TestMethod]
        public void When_Move_To_HortogonalLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 10));
            var actual = target.Points.ToArray();

            AssertCoords(new[]
            {
                new RayCoords(9, 10),
                new RayCoords(8, 10),
                new RayCoords(7, 10),
                new RayCoords(6, 10),
                new RayCoords(5, 10),
            }, actual);
             
        }

        [TestMethod]
        public void When_Move_To_HortogonalRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 10));
            var actual = target.Points.ToArray();
            
            AssertCoords(new[]
            {
                new RayCoords(11, 10),
                new RayCoords(12, 10),
                new RayCoords(13, 10),
                new RayCoords(14, 10),
                new RayCoords(15, 10),
            }, actual);
        }

        [TestMethod]
        public void When_Move_To_TopLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 5));
            var actual = target.Points.ToArray();
            AssertCoords(new[]
            {
                new RayCoords(9, 9),
                new RayCoords(8, 8),
                new RayCoords(7, 7),
                new RayCoords(6, 6),
                new RayCoords(5, 5),
            }, actual);
             
        }

        [TestMethod]
        public void When_Move_To_TopRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 5));
            var actual = target.Points.ToArray();


            AssertCoords(new[]
            {
                new RayCoords(11, 9),
                new RayCoords(12, 8),
                new RayCoords(13, 7),
                new RayCoords(14, 6),
                new RayCoords(15, 5),
            }, actual);
            
        }

        [TestMethod]
        public void When_Move_To_BottomRight()
        {
            var target = new Ray(new Coords(10, 10), new Coords(15, 15));
            var actual = target.Points.ToArray();

            AssertCoords(new[]
            {
                new RayCoords(11, 11), 
                new RayCoords(12, 12),
                new RayCoords(13, 13),
                new RayCoords(14, 14),
                new RayCoords(15, 15),
            }, actual);

        }

        [TestMethod]
        public void When_Move_To_BottomLeft()
        {
            var target = new Ray(new Coords(10, 10), new Coords(5, 15));
            var actual = target.Points.ToArray();
            
            AssertCoords(new []
            {
                new RayCoords(9, 11),
                new RayCoords(8, 12),
                new RayCoords(7, 13),
                new RayCoords(6, 14),
                new RayCoords(5, 15)
            }, actual);
        }

        private static void AssertCoords(RayCoords[] excepted, RayCoords[] actual)
        {
            Trace.WriteLine("excepted" + String.Join(" ",excepted.Select(x=> $"({x.X},{x.Y})")));
            Trace.WriteLine("actual" + String.Join(" ", actual.Select(x=> $"({x.X},{x.Y})")));

            Assert.AreEqual(excepted.Length, actual.Length);

            for (int i = 0; i < excepted.Length; i++)
            {
                Assert.IsTrue(CoordsUtils.Equals(excepted[i], actual[i]));       
            }
        }
    }
}