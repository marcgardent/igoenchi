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
                SaveAsAnalysis(string.Concat(path, "gga.csv"), analysis);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.In.ReadLine();
            }
        }

        private static IEnumerable<GoStat> RunAnalysis(GoGame game)
        {
            game.ToStart();

            var analyser = new Analyser();
            do
            {
                analyser.AnalyseStep(game.board);    
            }
            while ( game.ToNextMove());

            return analyser.Statistic;
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
