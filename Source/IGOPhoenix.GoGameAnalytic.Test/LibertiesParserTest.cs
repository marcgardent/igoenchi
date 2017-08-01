using System.Text;
using System.Collections.Generic;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.BitPlaneParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IGOPhoenix.GoGameAnalytic.Test
{
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
