using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.Analysis.SpeechOfStones.Models;

namespace IGOPhoenix.Analysis.SpeechOfStones
{
    public class SpeechAnalyser
    { 
        public SpeechModel AnalyseStep(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites)
        {
 
            var links = GetLinks(stone, blacks, whites).OrderBy(x => x.Distance).ToArray();
            var first = links.FirstOrDefault();
            if (first != null)
            {
                var firstRank = links.TakeWhile(x => x.Distance == first.Distance).ToArray();

                if (firstRank.Count() == 1 && links.Count() >= 2)
                {
                    var second = links.Skip(1).First();
                    var secondRank = links.Skip(1).TakeWhile(x => Math.Abs(x.Distance - second.Distance) < 0.4).ToArray();

                    return new SpeechModel(firstRank, secondRank);
                }
                else
                {
                    return new SpeechModel(firstRank, new List<LinkModel>());
                }
            }
            else
            {
                return new SpeechModel(new List<LinkModel>(), new List<LinkModel>());
            }            
        }

        private IEnumerable<LinkModel> GetLinks(ICoords stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites)
        {
            foreach (var coords in blacks)
            {
                if (!(coords.X == stone.X && coords.Y == stone.Y)) { 
                    yield return LinkModel.LinkFactory(stone, coords, true);
                }
            }

            foreach (var coords in whites)
            {
                if (!(coords.X == stone.X && coords.Y == stone.Y)) { 
                    yield return LinkModel.LinkFactory(stone, coords, false);
                }
            }
        }
    }
}
