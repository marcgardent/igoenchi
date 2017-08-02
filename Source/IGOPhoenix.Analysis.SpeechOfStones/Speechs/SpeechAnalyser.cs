using System.Collections.Generic;
using System.Linq;
using System.Text;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models;

namespace IGOPhoenix.Analysis.SpeechOfStones.Speechs
{


    public static class LabelHelper
    {
        private static readonly string XLabels = "ABCDEFGHJKLMNOPQRST";


        public static string CoordLabel(this ICoords coord)
        {
            return $"{XLabels[coord.X]}{(19 - coord.Y).ToString("00")}";
        }

        public static string ColorLabel(this Stone stone)
        {
            return stone.IsBlack ? "Black" : "White";
        }
    }

    public class SpeechAnalyser
    {
        const int MinimalDistanceToinvade = 8;

        public string Parse(Stone stone, IEnumerable<RankModel> ranks)
        {
            var sb = new StringBuilder();
            var verb = "playing";
            var triangle = SpeechAnalyser.TakeNearest(ranks, 2);

            sb.AppendLine("<speechOfStone>");
            if (!ranks.Any() || ranks.First().Distance > MinimalDistanceToinvade)
            {
                verb = "exploring";
                //TODO Mark zone

            }
            else if (triangle.All(x=> stone.IsBlack == x.Stone.IsBlack))// my stones
            {
                verb = "developping";
            }
            else if (triangle.All(x => stone.IsBlack != x.Stone.IsBlack))// thier stones
            {
                verb = "invading";
            }
            else
            {
                bool myStonesAreNear = triangle.Where(x => stone.IsBlack == x.Stone.IsBlack).All(x => x.Distance <= MinimalDistanceToinvade);
                bool yourStonesAreNear = triangle.Where(x => stone.IsBlack != x.Stone.IsBlack).All(x => x.Distance <= MinimalDistanceToinvade);
                

                if (myStonesAreNear && yourStonesAreNear) verb = "reducing";
                else if (!myStonesAreNear && !yourStonesAreNear) verb = "exploring!?";
                else if (myStonesAreNear && !yourStonesAreNear) verb = "expanding";
                else if (!myStonesAreNear && yourStonesAreNear) verb = "scouting";
                else
                {
                    verb = "expanding!?"; 
                }
            }
            
            sb.AppendLine(string.Format("{0} player is {2} at {1}", stone.ColorLabel(), stone.CoordLabel(), verb));
            
            sb.AppendLine("</speechOfStone>");

            return sb.ToString();
        }


        /// <summary>
        /// take N nearest stones but with whole hightest Rank.
        /// </summary>
        public static IEnumerable<LinkModel> TakeNearest(IEnumerable<RankModel> ranks, int n)
        {
            int retCount = 0;

            foreach (var rankModel in ranks)
            {
                foreach (var stone in rankModel.Stones)
                {
                    yield return new LinkModel(stone, rankModel.Distance);
                    retCount++;
                }

                if (retCount >= n) break;
            }
        }
    }
}