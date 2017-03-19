using System.Collections.Generic;

namespace IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models
{
    public class SpeechModel
    {
        public readonly IList<RankModel> Ranks;
        
        public SpeechModel(IList<RankModel> ranks)
        {
            Ranks = ranks;
        }
    }
}