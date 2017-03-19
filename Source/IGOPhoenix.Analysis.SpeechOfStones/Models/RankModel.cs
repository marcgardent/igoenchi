using System.Collections.Generic;
using IGOEnchi.GoGameLogic;

namespace IGOPhoenix.Analysis.SpeechOfStones.Models
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
    }
}