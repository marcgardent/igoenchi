using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
    [TestClass]
    public class InfluenceMapTest
    {
        [TestMethod]
        public void WhenOne()
        {
            var data = new BitPlane(5);
            data.Add(new Coords(3, 3));

            var target = new InfluenceMap(data);

            //todo Assert
        }
    }
}