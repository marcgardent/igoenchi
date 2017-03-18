using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOEnchi.GoGameLogic;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.io;
using IGOPhoenix.GoGameAnalytic;
using IGOPhoenix.GoGameAnalytic.Models;

namespace IGOPhoenix.GameAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 1)
            {
                Console.Out.WriteLine("excepted : GameAnalysis.exe  \"<path of .sgf>\"");
                return;
            }
            var path = args[0];

            try
            {
                var gogame = OpenFile(path);
                var analysis = RunAnalysis(gogame);
                SaveAsAnalysis(Path.ChangeExtension(path, ".csv"), analysis);

                var saveAsSgf = Path.ChangeExtension(path, ".analysis.sgf");
                SaveAsSgf(gogame, saveAsSgf);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.In.ReadLine();
            }
        }

        private static void SaveAsSgf(GoGame gogame, string path)
        {
            var builder = new GoSgfBuilder(gogame);
            var sgf = builder.ToSGFTree();

            using (var file = File.CreateText(path))
            {
                var writer = new SgfWriter(file, true);
                writer.WriteSgfTree(sgf);
            }
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

        private static IEnumerable<GoStat> RunAnalysis(GoGame game)
        {
            game.ToStart();

            var analyser = new Analyser();
            do
            {
                var stat = analyser.AnalyseStep(game.board);

                game.CurrentNode.Comment = BuildMessage(stat) + game.CurrentNode.Comment;

                MarkChain(game, stat.BlackStat);
                MarkChain(game, stat.WhiteStat);
            }
            while ( game.ToNextMove());

            return analyser.Statistic;
        }

        private static void MarkChain(GoGame game, GoPlayerStat stat)
        {
            game.CurrentNode.EnsureMarkup();
            
            foreach (var grp in stat.Groups.OrderByDescending(x => x.StoneCount))
            {
                foreach (var chain in grp.Chains.OrderByDescending(x=> x.StoneCount))
                {
                    foreach (var stone in chain.Stones.Unabled)
                    {
                        game.CurrentNode.Markup.Labels.Add(new TextLabel(stone.X, stone.Y, $"L{chain.LibertyCount}"));
                    }
                }
            }
        }

        private static void MarkGroup(GoGame game, GoPlayerStat stat)
        {
            game.CurrentNode.EnsureMarkup();

            string grpLabel = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var index = 0;
            foreach (var grp in stat.Groups.OrderByDescending(x=> x.StoneCount))
            {
                var letter = grpLabel[index%grpLabel.Length].ToString();
                foreach (var stone in grp.Stones.Unabled)
                {
                    game.CurrentNode.Markup.Labels.Add(new TextLabel(stone.X, stone.Y, letter));
                }
                index++;
            }
        }

        private static void SaveAsAnalysis(string path, IEnumerable<GoStat> statistics)
        {
            using (var s = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(s))
            {
                
                writer.Write("Turn;Black Stones;Black Groups;Black liberties;");
                writer.WriteLine("White Stones;White Groups;White liberties;");
                foreach (var goStat in statistics)
                {
                    writer.Write($"{goStat.Turn};");
                    WriteGroup(writer, goStat.BlackStat);
                    WriteGroup(writer, goStat.WhiteStat);
                    writer.WriteLine();
                }
            }
        }

        private static void WriteGroup(StreamWriter writer, GoPlayerStat stat)
        {
            writer.Write($"{stat.StoneCount};{ stat.Groups.Count};{stat.LibertyCount};");
        }

        private static GoGame OpenFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var excepted = SgfReader.LoadFromStream(stream);
                return SgfCompiler.Compile(excepted);
            }
        }
    }
}
