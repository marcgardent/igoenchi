using System;
using System.Collections.Generic;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting; 

namespace IGOPhoenix.GoGameAnalytic.Test
{
    [TestClass]
    public class GroupParserTest
    {
        [TestMethod]
        public void When_Empty_BitPlane_Do_EmptyList()
        {
            var target = GroupParser.NobiGroupParser();
            var data = new BitPlane(19);

            var actual = target.Parse(data);
            
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void When_Singleton_Stones_Do_It()
        {
            var target = GroupParser.NobiGroupParser();
            var data = new BitPlane(19);
            data.Add(new Coords(1, 1));
            data.Add(new Coords(2, 2));
            data.Add(new Coords(3, 3));
            data.Add(new Coords(4, 4));
            data.Add(new Coords(5, 5));
            var actual = target.Parse(data);

            Assert.AreEqual(5, actual.Count);
        }

        [TestMethod]
        public void When_One_Group_Do_It()
        {
            var target = GroupParser.NobiGroupParser();
            var grpA = new BitPlane(10);
            grpA.Add(new Coords(1, 1));
            grpA.Add(new Coords(1, 2));
            grpA.Add(new Coords(1, 3));
            grpA.Add(new Coords(1, 4));

            var actual = target.Parse(grpA.Copy());
            Assert.AreEqual(1, actual.Count);
            BitPlaneTestHelper.AssertBitPlane(grpA, actual[0] , "Group A");
        }
        
        [TestMethod]
        public void When_Two_Group_Do_It()
        {
            var target = GroupParser.NobiGroupParser();
            var grpA = new BitPlane(10);
            grpA.Add(new Coords(1, 1));
            grpA.Add(new Coords(1, 2));
            grpA.Add(new Coords(1, 3));
            grpA.Add(new Coords(1, 4));

            var grpB = new BitPlane(10);
            grpB.Add(new Coords(4, 1));
            grpB.Add(new Coords(4, 2));
            grpB.Add(new Coords(4, 3));
            grpB.Add(new Coords(4, 4));
            
            var data = new BitPlane(10);
            data.Or(grpA);
            data.Or(grpB);
            
            var actual = target.Parse(data.Copy());
            Assert.AreEqual(2, actual.Count);
            BitPlaneTestHelper.AssertBitPlane(grpA, actual[0], "Group A"); //Todo Algorithm : Make to Order Tolerance
            BitPlaneTestHelper.AssertBitPlane(grpB, actual[1], "Group B");
        }


    }
}
