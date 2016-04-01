using System;

namespace IGoEnchi
{
    public class IGSRank : IComparable
    {
        private IGSRankClass rankClass;
        private readonly string rankString;
        private int rankValue;

        public IGSRank(string rank)
        {
            if (rank == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }
            rankString = rank;
            ParseString(rank);
        }

        public int CompareTo(object rank)
        {
            if (!(rank is IGSRank))
            {
                throw new ArgumentException("Argument is not of type IGSRank");
            }
            var otherRank = rank as IGSRank;

            if (rankClass != otherRank.rankClass)
            {
                switch (rankClass)
                {
                    case IGSRankClass.Pro:
                        return 1;
                    case IGSRankClass.Kyu:
                        return -1;
                    default:
                        switch (otherRank.rankClass)
                        {
                            case IGSRankClass.Pro:
                                return -1;
                            case IGSRankClass.Kyu:
                                return 1;
                        }
                        break;
                }
            }
            else
            {
                if (rankClass == IGSRankClass.Kyu)
                {
                    return (-rankValue).CompareTo(-otherRank.rankValue);
                }
                return rankValue.CompareTo(otherRank.rankValue);
            }

            return 0;
        }

        private void ParseString(string rank)
        {
            var index = 0;

            rankValue = int.MaxValue;
            rankClass = IGSRankClass.Kyu;

            while ((index < rank.Length) && char.IsDigit(rank[index]))
            {
                index += 1;
            }

            if (index > 0)
            {
                rankValue = Convert.ToInt32(rank.Substring(0, index));
                if (index < rank.Length)
                {
                    switch (rank[index])
                    {
                        case 'k':
                            rankClass = IGSRankClass.Kyu;
                            break;
                        case 'd':
                            rankClass = IGSRankClass.Dan;
                            break;
                        case 'p':
                            rankClass = IGSRankClass.Pro;
                            break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return rankString;
        }
    }
}