using System.Linq;
using System.Text;
using IGOEnchi.GoGameLogic;
using IGOPhoenix.GoGameAnalytic.Models;

namespace IGOPhoenix.GameAnalysis.views
{
    /// <summary>
    /// Adapt GoPlayerStat to GoGameLogic
    /// </summary>
    public class SgfView
    {
        private readonly GoNode Node;
        private readonly GoStat stat;

        public SgfView(GoNode node, GoStat stat)
        {
            Node = node;
            this.stat = stat;
        }
        
        public void MarkBlackChainWithLiberties()
        {
            this.MarkChainWithLiberties(stat.BlackStat);
        }

        public void MarkWhiteChainWithLiberties()
        {
            this.MarkChainWithLiberties(stat.WhiteStat);
        }

        public void MarkBlackGroupWithLetter()
        {
            this.MarkGroupWithLetter(stat.BlackStat);
        }

        public void MarkWhiteGroupWithLetter()
        {
            this.MarkGroupWithLetter(stat.WhiteStat);
        }

        private void MarkChainWithLiberties(GoPlayerStat playerStat)
        {
            Node.EnsureMarkup();

            foreach (var grp in playerStat.Groups.OrderByDescending(x => x.StoneCount))
            {
                foreach (var chain in grp.Chains.OrderByDescending(x => x.StoneCount))
                {
                    foreach (var stone in chain.Stones.Unabled)
                    {
                        Node.Markup.Labels.Add(new TextLabel(stone.X, stone.Y, $"L{chain.LibertyCount}"));
                    }
                }
            }
        }

        private void MarkGroupWithLetter(GoPlayerStat playerStat)
        {
            Node.EnsureMarkup();

            string grpLabel = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var index = 0;
            foreach (var grp in playerStat.Groups.OrderByDescending(x => x.StoneCount))
            {
                var letter = grpLabel[index % grpLabel.Length].ToString();
                foreach (var stone in grp.Stones.Unabled)
                {
                    Node.Markup.Labels.Add(new TextLabel(stone.X, stone.Y, letter));
                }
                index++;
            }
        }
        
        public void AppendStatComment()
        {
            Node.Comment = BuildMessage(stat) + Node.Comment;
        }

        private static string BuildMessage(GoStat stat)
        {
            var b = new StringBuilder();

            b.AppendLine("<Analysis>");
            b.AppendLine($"turn : {stat.Turn}");
            b.AppendLine();
            b.AppendLine("Liberties");
            b.AppendLine($"Count : B{stat.BlackStat.LibertyCount} W{stat.WhiteStat.LibertyCount}");
            b.AppendLine($"Efficiency: B{stat.BlackStat.EfficiencyOfLiberties:##.00}% W{stat.WhiteStat.EfficiencyOfLiberties:##.00}%");
            b.AppendLine();
            b.AppendLine("Structures");
            b.AppendLine($"Groups (nobi+kosumi) : B{stat.BlackStat.Groups.Count} W{stat.WhiteStat.Groups.Count}");
            b.AppendLine($"Chains (nobi) : B{stat.BlackStat.ChainCount} W{stat.WhiteStat.ChainCount}");
            b.AppendLine("</Analysis>");
            return b.ToString();
        }
    }
}