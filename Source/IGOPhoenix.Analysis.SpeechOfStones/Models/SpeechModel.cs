using System.Collections.Generic;

namespace IGOPhoenix.Analysis.SpeechOfStones.Models
{
    
    public class SpeechModel
    {
        public readonly IEnumerable<LinkModel> FirstRank;
        public readonly IEnumerable<LinkModel> SecondRank;

        public SpeechModel(IEnumerable<LinkModel> firstRank, IEnumerable<LinkModel> secondRank)
        {
            FirstRank = firstRank;
            SecondRank = secondRank;
        }
    }
}