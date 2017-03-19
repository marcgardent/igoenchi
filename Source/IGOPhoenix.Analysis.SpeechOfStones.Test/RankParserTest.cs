using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models;
using NUnit.Framework;

namespace IGOPhoenix.Analysis.SpeechOfStones.Test
{
    
    [TestFixture]
    public class RankParserTest
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
        [Order(0), Test, TestCaseSource(typeof (RankParserTest), nameof(RankParserTest.Do_No_Ranks_Cases))]
        public void Do_No_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            Assert.AreEqual(0, actual.Count);
        }


        public struct RankAssert
        {
            public int BlackCount;
            public int WhiteCount;
            public byte Distance;
        }

        public static IEnumerable Do_Parse_Ranks_Cases
        {
            get
            {
                yield return
                    new TestCaseData(new Stone(20, 10, false),
                        new List<ICoords>() {new Coords(10, 10)},
                        new List<ICoords>() {new Coords(20, 10)},
                        new[] { new RankAssert() { Distance = 10, BlackCount = 1, WhiteCount = 0} })
                    .SetName("When play second turn of game");

                yield return
                    new TestCaseData(new Stone(10, 10, true),
                        new List<ICoords>() { new Coords(10, 10), new Coords(10, 20) },
                        new List<ICoords>() { new Coords(20, 10), new Coords(0, 10) },
                        new[] { new RankAssert() { Distance = 10, BlackCount = 1, WhiteCount = 2 } })
                    .SetName("When Black and White stones in a Rank");

                yield return
                    new TestCaseData(new Stone(10, 10, false),
                        new List<ICoords>() { new Coords(20, 10), new Coords(0, 10) },
                        new List<ICoords>() { new Coords(10, 10), new Coords(10, 20) },
                        new[] { new RankAssert() { Distance = 10, BlackCount = 2, WhiteCount = 1 } })
                    .SetName("When White and Black stones in  a Rank");


                yield return
                    new TestCaseData(new Stone(20, 10, false),
                        new List<ICoords>() {new Coords(10, 10), new Coords(20, 20) },
                        new List<ICoords>() {new Coords(20, 10), new Coords(30, 10) },
                        new[] { new RankAssert() {Distance = 10, BlackCount = 2, WhiteCount = 1}})
                    .SetName("When are only one rank");

                yield return
                    new TestCaseData(new Stone(10, 10, true),
                    new List<ICoords>() { new Coords(10, 10) },
                    new List<ICoords>() { new Coords(20, 10), new Coords(30, 10), new Coords(40, 10) },
                    new[] {
                        new RankAssert() { Distance = 10, BlackCount = 0, WhiteCount = 1 },
                        new RankAssert() { Distance = 20, BlackCount = 0, WhiteCount = 1 },
                        new RankAssert() { Distance = 30, BlackCount = 0, WhiteCount = 1 } 
                    })
                    .SetName("When there are three ranks");
            }
        }

        [Order(1), Test, TestCaseSource(typeof (RankParserTest), nameof(RankParserTest.Do_Parse_Ranks_Cases))]
        public void Do_Count_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites, RankAssert[] excepted)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            Assert.AreEqual(excepted.Length, actual.Count);
        }


        [Order(2), Test, TestCaseSource(typeof(RankParserTest), nameof(RankParserTest.Do_Parse_Ranks_Cases))]
        public void Do_Distance_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites,RankAssert[] excepted)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            for (int i = 0; i < actual.Count; i++)
            {
                RankAssert exceptedRank = excepted[i];
                RankModel actualRank = actual[i];
                Assert.AreEqual(exceptedRank.Distance, actualRank.Distance, $"Distance Rank {i}");
            }
        }

        [Order(2), Test, TestCaseSource(typeof(RankParserTest), nameof(RankParserTest.Do_Parse_Ranks_Cases))]
        public void Do_TotalCount_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites, RankAssert[] excepted)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            for (int i = 0; i < actual.Count; i++)
            {
                RankAssert exceptedRank = excepted[i];
                RankModel actualRank = actual[i];
                Assert.AreEqual(exceptedRank.WhiteCount + exceptedRank.BlackCount, actualRank.Stones.Count(), $"TotalCount Rank #{i + 1}");
            }
        }

        [Order(3), Test, TestCaseSource(typeof(RankParserTest), nameof(RankParserTest.Do_Parse_Ranks_Cases))]
        public void Do_BlackCount_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites, RankAssert[] excepted)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            for (int i = 0; i < actual.Count; i++)
            {
                RankAssert exceptedRank = excepted[i];
                RankModel actualRank = actual[i];
                Assert.AreEqual(exceptedRank.BlackCount, actualRank.Stones.Count(x => x.IsBlack), $"BlackCount Rank #{i + 1}");
            }
        }

        [Order(3), Test, TestCaseSource(typeof(RankParserTest), nameof(RankParserTest.Do_Parse_Ranks_Cases))]
        public void Do_WhiteCount_Ranks(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites, RankAssert[] excepted)
        {
            var target = new RankParser();
            var actual = target.Parse(stone, blacks, whites);

            for (int i = 0; i < actual.Count; i++)
            {
                RankAssert exceptedRank = excepted[i];
                RankModel actualRank = actual[i];
                Assert.AreEqual(exceptedRank.WhiteCount, actualRank.Stones.Count(x => !x.IsBlack), $"WhiteCount Rank #{i + 1}");
            }
        }
    }
}
