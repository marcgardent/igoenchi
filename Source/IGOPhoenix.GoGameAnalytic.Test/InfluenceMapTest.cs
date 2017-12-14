using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
    [TestClass]
    public class InfluenceMapBuilderTest
    {
        [TestMethod]
        public void When_Execute()
        {
            var data = new BitPlane(5);
            data.Add(new Coords(3, 3));

            var target = new InfluenceMapBuilder(data);
            //todo Assert
        }

        [TestMethod]
        public void When_Walking_OilStain_No_Infinity_loop()
        {
            var target = AnalyticHelper.OilStainWalker(new BitPlane(11), new Coords(5, 5));
            var actual = target.ToArray();
        }

        [TestMethod]
        public void When_Walking_OilStain_On_Center_Should_Have_Symetric_Property()
        {
            var target = AnalyticHelper.OilStainWalker(new BitPlane(3), new Coords(1, 1));
            var actual = target.ToArray();
            foreach (var points in actual)
            {
                Assert.AreEqual(0,points.Count()%4);
            }
        }

        [TestMethod]
        public void LocalInfluence()
        {
            var b = new Board(19);
            b.White[3, 3] = true;
            b.White[6, 2] = true;
            b.White[6, 5] = true;
            b.Black[7, 3] = true;

            var target = new LocalInfluence(b);
            BitPlaneTestHelper.TraceBitPlane(target.White, "white");
            BitPlaneTestHelper.TraceBitPlane(target.Black, "black");

            Assert.AreEqual(false, target.White[6, 3]);
            Assert.AreEqual(false, target.Black[6, 3]);
            Assert.AreEqual(true, target.White[5, 3]);

        }

        [TestMethod]
        public void When_Circle()
        {
            
        }
    }
}