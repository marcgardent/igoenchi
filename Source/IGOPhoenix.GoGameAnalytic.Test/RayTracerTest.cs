using System;
using System.Diagnostics;
using System.Globalization;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.Maps;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
    [TestClass]
    public class RayTracerTest
    {
        [TestMethod]
        public void When_distanceToLine()
        {
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(1, 6)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(2, 6)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(3, 6)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(4, 6)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(5, 6)));

            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(1, 2)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(2, 2)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(3, 2)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(4, 2)));
            Assert.AreEqual(2, CoordsUtils.distanceToLine(new Coords(3, 4), new Coords(5, 4), new Coords(5, 2)));
        }

        [TestMethod]
        public void When_Execute_AloneCase()
        {
            var light = new BitPlane(3,4);
            light.Add(new Coords(1, 1));
            
            var target = new RayTracer(light, light);
            var actual = target.GetMap();

            var excepted = new Map(3, 4)
            {
                [0, 0] = 0.5,
                [0, 1] = 1,
                [0, 2] = 0.5,
                [0, 3] = 0.333333333333333,
                [1, 0] = 1,
                [1, 1] = 0,
                [1, 2] = 1,
                [1, 3] = 0.5,
                [2, 0] = 0.5,
                [2, 1] = 1,
                [2, 2] = 0.5,
                [2, 3] = 0.333333333333333
            };
            AssertMap(excepted, actual);
        }
        
        [TestMethod]
        public void When_Execute_With_ShadowRight()
        {
            var light = new BitPlane(3,4);
            light.Add(new Coords(0, 1));
            var solid = light.Copy();
            solid.Add(new Coords(0, 2));
            var target = new RayTracer(light, solid);
            var actual = target.GetMap();
            var excepted = new Map(3, 4)
            {
                [0, 0] = 1,
                [0, 1] = 0,
                [0, 2] = 0,
                [0, 3] = 0,
                [1, 0] = 0.5,
                [1, 1] = 1,
                [1, 2] = 0,
                [1, 3] = 0,
                [2, 0] = 0.333333333333333,
                [2, 1] = 0.5,
                [2, 2] = 0.333333333333333,
                [2, 3] = 0
            };
            AssertMap(excepted, actual);
        }


        [TestMethod]
        public void When_Execute_With_ShadowLeft()
        {
            var light = new BitPlane(3, 4);
            light.Add(new Coords(0, 2));
            var solid = light.Copy();
            solid.Add(new Coords(0, 1));
            var target = new RayTracer(light, solid);
            var actual = target.GetMap();
            var excepted = new Map(3, 4)
            {
                [0, 0] = 0,
                [0, 1] = 0,
                [0, 2] = 0,
                [0, 3] = 1,
                [1, 0] = 0,
                [1, 1] = 0,
                [1, 2] = 1,
                [1, 3] = 0.5,
                [2, 0] = 0,
                [2, 1] = 0.333333333333333,
                [2, 2] = 0.5,
                [2, 3] = 0.333333333333333
            };
            AssertMap(excepted, actual);
        }

        [TestMethod]
        public void When_Execute_With_ShadowTop()
        {
            var light = new BitPlane(3, 4);
            light.Add(new Coords(2, 0));
            var solid = light.Copy();
            solid.Add(new Coords(1, 0));
            var target = new RayTracer(light, solid);
            var actual = target.GetMap();
            var excepted = new Map(3, 4)
            {
                [0, 0] = 0,
                [0, 1] = 0,
                [0, 2] = 0,
                [0, 3] = 0.2,
                [1, 0] = 0,
                [1, 1] = 0,
                [1, 2] = 0.333333333333333,
                [1, 3] = 0.25,
                [2, 0] = 0,
                [2, 1] = 1,
                [2, 2] = 0.5,
                [2, 3] = 0.333333333333333
            };
            AssertMap(excepted, actual);
        }

        [TestMethod]
        public void When_Execute_With_ShadowBottom()
        {
            var light = new BitPlane(3, 4);
            light.Add(new Coords(0, 1));
            var solid = light.Copy();
            solid.Add(new Coords(1, 1));
            var target = new RayTracer(light, solid);
            var actual = target.GetMap();
            var excepted = new Map(3, 4)
            {
                [0, 0] = 1,
                [0, 1] = 0,
                [0, 2] = 1,
                [0, 3] = 0.5,
                [1, 0] = 0,
                [1, 1] = 0,
                [1, 2] = 0,
                [1, 3] = 0.333333333333333,
                [2, 0] = 0,
                [2, 1] = 0,
                [2, 2] = 0,
                [2, 3] = 0
            };
            AssertMap(excepted, actual);
        }

        private static void WriteCmd(Map actual)
        {

            Trace.WriteLine($"var excepted =new Map({actual.Width}, {actual.Height});");

            for (int x = 0; x < actual.Width; x++)
            {
                for (int y = 0; y < actual.Height; y++)
                {
                    Trace.Write($"excepted[{x},{y}]={actual[x,y].ToString("G",CultureInfo.InvariantCulture)};\t");
                }
                Trace.WriteLine("");
            }

            Trace.WriteLine("AssertMap(excepted, actual);");
        }

        void AssertMap(Map excepted, Map actual)
        {
            WriteMap(excepted);
            WriteMap(actual);
            WriteCmd(actual);

            foreach (var allCoord in excepted.AllCoords)
            {
                var a = Math.Abs(excepted[allCoord] - actual[allCoord]) < 0.00000000000001;
                
                Assert.IsTrue(a,$"excepted : '{excepted[allCoord]}', actual : '{actual[allCoord]}'");

            }
        }

        private static void WriteMap(Map actual)
        {
            Trace.Write("    ");
            for (int y = 0; y < actual.Height; y++)
            {
                Trace.Write($"({y})\t\t");
            }

            Trace.WriteLine("");
            for (int x = 0; x < actual.Width; x++)
            {
                Trace.Write($"({x}) ");
                for (int y = 0; y < actual.Height; y++)
                {
                    Trace.Write($"{actual[x, y]:F}\t");
                }
                Trace.WriteLine("");
            }
        }
    }
}