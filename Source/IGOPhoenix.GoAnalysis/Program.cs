using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IGOEnchi.GoGameLogic;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.io;
//using IGOPhoenix.Analysis.SpeechOfStones;
using IGOPhoenix.GameAnalysis.views;
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
                var csv = new CsvView(analysis);
                csv.ToCsv(Path.ChangeExtension(path, ".csv"));
                
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
        
        private static IEnumerable<GoStat> RunAnalysis(GoGame game)
        {
            game.ToStart();

            var analyser = new Analyser();
            //var speechAnalyser = new SpeechAnalyser();
            do
            {
                var stat = analyser.AnalyseStep(game.board);


                /*
                var view = new SgfView(game.CurrentNode, stat);
                view.AppendStatComment();
                view.MarkWhiteChainWithLiberties();
                view.MarkBlackChainWithLiberties();
                */

                var move =game.CurrentNode as GoMoveNode;
                /*
                if (move!=null)
                {
                    var result = speechAnalyser.AnalyseStep(move.Stone, game.board);
                }*/
            }

            while ( game.ToNextMove());

            return analyser.Statistic;
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
