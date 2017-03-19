using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models;

namespace IGOPhoenix.Analysis.SpeechOfStones.Ranks
{
    public class RankParser
    { 
        public IList<RankModel> Parse(Stone stone, IEnumerable<ICoords> blacks, IEnumerable<ICoords> whites)
        {
 
            var links = GetLinks(stone, blacks, whites).OrderBy(x => x.Distance).ToArray();
            var ranks= links
                    .GroupBy(x=> x.Distance)
                    .Select(x=> new RankModel(x.Select(s=>s.Stone).ToArray(), x.Key))
                    .OrderBy(x=>x.Distance).ToList();

            return ranks;
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
