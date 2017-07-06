using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IGOEnchi.GoGameLogic;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.io;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks;
using IGOPhoenix.Analysis.SpeechOfStones.Ranks.Models;
using IGOPhoenix.Analysis.SpeechOfStones.Speechs;
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
            var speechAnalyser = new SpeechAnalyser();
            var rankParser = new RankParser();
            do
            {
                /*
                var stat = analyser.AnalyseStep(game.board);
                var view = new SgfView(game.CurrentNode, stat);
                view.AppendStatComment();
                view.MarkWhiteChainWithLiberties();
                view.MarkBlackChainWithLiberties();
                */

                var move =game.CurrentNode as GoMoveNode;
                
                if (move!=null)
                {
                    var ranks = rankParser.Parse(move.Stone, game.board.Black.Unabled, game.board.White.Unabled);
                    MarkEdgeRanks(move, ranks);
                    MarkRanks(move, ranks);
                    var message = speechAnalyser.Parse(move.Stone, ranks);
                    move.Comment = message + move.Comment;
                }
            }

            while ( game.ToNextMove());

            return analyser.Statistic;
        }

        private static void MarkRanks(GoMoveNode Node, IList<RankModel> ranks)
        {
            Node.EnsureMarkup();

        
            foreach (var rank in ranks)
            {
                foreach (var stone in rank.Stones)
                {
                    Node.Markup.Labels.Add(new TextLabel(stone.X, stone.Y, $"{rank.Distance}"));
                    
                }
            }
        }

        private static void MarkEdgeRanks(GoMoveNode Node, IList<RankModel> ranks)
        {
            Node.EnsureMarkup();
            int count = 0;
            foreach (var rank in ranks)
            {
                foreach (var stone in rank.Stones)
                {
                    foreach (var lineCoord in LineCoords(Node.Stone, stone))
                    {
                        Node.Markup.Marks.Add(new Mark(lineCoord.X, lineCoord.Y, MarkType.Square));
                        count++;
                    }
                    if (count >= 4) return;
                }
            }
        }


        public static IEnumerable<ICoords> LineCoords(ICoords start,ICoords stop)
        {
            int dx = (byte) Math.Abs(stop.X - start.X);
            int sx = start.X < stop.X ? 1 : -1;
            int dy = -Math.Abs(stop.Y - start.Y);
            int sy = start.Y < stop.Y ? 1 : -1;
            int err = dx + dy, e2; // error value e_xy
            int xlocal = start.X;
            int ylocal = start.Y;
            for (;;)
            {
                yield return new Coords((byte)xlocal, (byte)ylocal);

                if (xlocal == stop.X && ylocal == stop.Y) break;

                e2 = 2 * err;

                // horizontal step?
                if (e2 > dy)
                {
                    err += dy;
                    xlocal += sx;
                }

                // vertical step?
                if (e2 < dx)
                {
                    err += dx;
                    ylocal += sy;
                }
            }
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
