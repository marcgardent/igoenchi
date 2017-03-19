using System.Collections;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.Analysis.SpeechOfStones.Models;
using NUnit.Framework;

namespace IGOPhoenix.Analysis.SpeechOfStones.Test
{
    [TestFixture]
    public class LinkFactoryTest
    {
        public static IEnumerable Do_Correct_Distance_Cases
        {
            get
            {
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(10, 10)).SetName("Zero point").Returns(0);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(20, 10)).SetName("+X padding").Returns(10);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(5, 10)).SetName("-X padding").Returns(5);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(10, 20)).SetName("+Y padding").Returns(10);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(10, 5)).SetName("-Y padding").Returns(5);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(15, 20)).SetName("+X+Y padding").Returns(15);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(8, 5)).SetName("-X-Y padding").Returns(7);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(8, 20)).SetName("-X+Y padding").Returns(12);
                yield return new TestCaseData(new Stone(10, 10, true), new Coords(20, 8)).SetName("+X-Y padding").Returns(12);
            }
        }

        [Test, TestCaseSource(typeof (LinkFactoryTest), nameof(LinkFactoryTest.Do_Correct_Distance_Cases))]
        public byte Do_Correct_Distance(Stone stone, ICoords coords)
        {
            var actual = LinkModel.LinkFactory(stone, coords, true);
            return actual.Distance;
        }
    }
}