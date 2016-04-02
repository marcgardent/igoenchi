using System;
using System.Linq;
using IGOEnchi.GoGameLogic.Models;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOEnchi.GoGame.UnitTest
{
    [TestClass]
    public class GoSgfBuilderTest
    {
        [TestMethod]
        public void When_overflow_Coord_Do_Empty_Coord()
        {
            var g = new GoGameLogic.Models.GoGame(19);
            g.PlaceStone(new Stone(20,20, true));

            var target = new GoSgfBuilder(g);
            var result = target.ToSGFTree();

            var actual = result.ChildNodes[0].Properties.SingleOrDefault(x => x.Name == "B");
            Assert.IsNotNull(actual); 
            Assert.AreEqual(String.Empty, actual.Value);
        }
    }
}
