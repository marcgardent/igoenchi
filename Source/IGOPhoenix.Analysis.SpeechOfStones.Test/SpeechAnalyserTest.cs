using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using NUnit.Framework;

namespace IGOPhoenix.Analysis.SpeechOfStones.Test
{
    
    [TestFixture]
    public class SpeechAnalyserTest
    {

        public static IEnumerable Do_No_Ranks_Cases
        {
            get
            {
                yield return
                    new TestCaseData(new Stone(10, 10, true), new List<ICoords>() {new Coords(10, 10)},
                        new List<ICoords>())
                        .SetName("When first stone:Black, board:black");
                yield return
                    new TestCaseData(new Stone(10, 10, false), new List<ICoords>() {new Coords(10, 10)},
                        new List<ICoords>())
                        .SetName("When first stone:White, board:black");
                yield return
                    new TestCaseData(new Stone(10, 10, true), new List<ICoords>(),
                        new List<ICoords>() {new Coords(10, 10)})
                        .SetName("When first stone:Black, board:White");
                yield return
                    new TestCaseData(new Stone(10, 10, false), new List<ICoords>(),
                        new List<ICoords>() {new Coords(10, 10)})
                        .SetName("When first stone:White, board:White");
            }
        }

        [Test, TestCaseSource(typeof (SpeechAnalyserTest), nameof(SpeechAnalyserTest.Do_No_Ranks_Cases))]
        public void Do_No_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites)
        {
            var target = new SpeechAnalyser();
            var actual = target.AnalyseStep(stone, blacks, whites);

            Assert.AreEqual(0, actual.FirstRank.Count());
            Assert.AreEqual(0, actual.SecondRank.Count());
        }


        public struct RankAssert
        {
            public int firstRankCount;
            public byte firstRankDistance;
            public int secondRankCount;
            public byte secondRankDistance;
        }

        public static IEnumerable Do_Good_Distance_And_Count_Ranks_Cases
        {
            get
            {
                yield return
                    new TestCaseData(new Stone(20, 10, false),
                        new List<ICoords>() {new Coords(10, 10)},
                        new List<ICoords>() {new Coords(20, 10)},
                        new RankAssert(){ firstRankCount = 1, firstRankDistance = 10, secondRankCount = 0, secondRankDistance = 0 })
                    .SetName("Second turn of game");

                yield return
                    new TestCaseData(new Stone(20, 10, true),
                    new List<ICoords>() { new Coords(10, 10), new Coords(30, 10) },
                    new List<ICoords>() { new Coords(20, 10) },
                    new RankAssert() { firstRankCount = 2, firstRankDistance = 10, secondRankCount = 0, secondRankDistance = 0 })
                    .SetName("When all stones are first rank");

                yield return
                    new TestCaseData(new Stone(10, 10, true),
                    new List<ICoords>() { new Coords(10, 10) },
                    new List<ICoords>() { new Coords(20, 10), new Coords(30, 10), new Coords(40, 10) },
                    new RankAssert() { firstRankCount = 1, firstRankDistance = 10, secondRankCount = 1, secondRankDistance = 20 })
                    .SetName("When there are third rank");
            }
        }

        [Test, TestCaseSource(typeof(SpeechAnalyserTest), nameof(SpeechAnalyserTest.Do_Good_Distance_And_Count_Ranks_Cases))]
        public void Do_Good_Distance_And_Count_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites, RankAssert asserts)
        {
            var target = new SpeechAnalyser();
            var actual = target.AnalyseStep(stone, blacks, whites);
            Assert.AreEqual(asserts.firstRankCount, actual.FirstRank.Count());
            Assert.AreEqual(asserts.secondRankCount, actual.SecondRank.Count());

            Assert.IsFalse(actual.FirstRank.Any(x => x.Distance!= asserts.firstRankDistance), $"Excepted FirstRank ={asserts.firstRankDistance}");
            Assert.IsFalse(actual.SecondRank.Any(x => x.Distance!= asserts.secondRankDistance), $"Excepted SecondRank ={asserts.secondRankDistance}");
        }
    }
}
