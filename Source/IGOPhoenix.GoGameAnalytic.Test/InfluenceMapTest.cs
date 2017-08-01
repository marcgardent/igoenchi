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
    }
}