using System.Collections.Generic;
using System.Linq;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models
{
    public class RankModel
    {
        public readonly IEnumerable<Stone> Stones;
        public readonly byte Distance;

        public RankModel(IEnumerable<Stone> stones, byte distance)
        {
            Stones = stones;
            Distance = distance;
        }

        public bool AllIsBlack => this.Stones.All(x => x.IsBlack);
        public bool AllIsWhite => this.Stones.All(x=>x.IsWhite);

        public bool AllIs(bool isBlack) => this.Stones.All(x => x.IsBlack == isBlack);
    }
}
