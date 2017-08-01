using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Fclp;
using GoART.FfmpegIntegration;
using GoART.NAudioIntegration;
using IGOEnchi.GoGameLogic;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using IGOEnchi.Videocast.Rendering;
using IGOEnchi.Videocast.Rendering.NativeImpl;
using IGOEnchi.Videocast.Rendering.NativeImpl.Models;
using IGOPhoenix.GoGameAnalytic.Maps.Influence.RayTracing;

namespace IGOEnchi.Videocast
{

    public class GoArtEnvironement
    {
        public string Input { get; set; }
        public string Temp { get; set; }
        public string Output { get; set; }
    }

    internal class Program
    {
        private static FluentCommandLineParser<GoArtEnvironement> GoArtEnvironementCliMap()
        {
            var p = new FluentCommandLineParser<GoArtEnvironement>();
            p.Setup(x => x.Input).As("input").Required().WithDescription("input : Sgf File");
            p.Setup(x => x.Output).As("output").Required().WithDescription("output : MP4 file");
            p.Setup(x => x.Temp).As("temp").WithDescription("working folder").SetDefault(System.IO.Path.GetTempPath());

            return p;
        }

        private static void Main(string[] args)
        {
            // ex. --input "C:\Users\Marc\Desktop\parties GO\Meijin-2015\meijin-2015-1.sgf" --output "D:\temp\video\meijin-2015-1.mp4" --temp "D:\temp\video\"

            var p = GoArtEnvironementCliMap();
            var r = p.Parse(args);

            if (r.HasErrors)
            {
                Console.WriteLine(r);
            }
            else
            {
                Main(p.Object);
            }
        }

        private static void Main(GoArtEnvironement env)
        {

            var game = OpenFile(env.Input);
            var resolution = 120; //release 240
            var samplerate = 8000; //release 48000
            var framerate = 4;
            var bar = 1; //measure
            var whole = 1.0/framerate; // time of whole
            using (var temp = new TempFileProvider(env.Temp))
            {
                var encoding = new FfmpegEncoding(temp, framerate);

                using (var wav = encoding.WavStream())
                using (GobanComposer goban = new GobanComposer(GobanColor.BlackTheme, game.BoardSize, resolution))
                {
                    game.ToStart();
                    var count = game.EnumerateMoves().Count();

                    var t = new EqualTemperamentTonesProvider();
                    var d = new DurationNoteProvider(TimeSpan.FromSeconds(whole));
                    var c = new MusicalCompositor(TimeSpan.FromSeconds(whole*count*bar), samplerate);
                    var m = new Metronome(TimeSpan.FromSeconds(whole));
                    var main = new DiatonicScaleTonesProvider(t, 4);
                    var second = new DiatonicScaleTonesProvider(t, 2);

                    game.ToStart();

                    GoMoveNode lastmove = null;
                    foreach (var move in game.EnumerateMoves())
                    {
                        goban.ClearGoban();

                        /////////////////////////////////////////////////////////////////////
                        var solids = game.board.BlackAndWhite;
                        
                        //var whiteMap = new InfluenceMapBuilder(game.board.White).GetMap();
                        //var blackMap = new InfluenceMapBuilder(game.board.Black).GetMap();

                        var whiteMap = new RayTracer(game.board.White, solids).GetMap();
                        var blackMap = new RayTracer(game.board.Black, solids).GetMap();

                        foreach (var coord in game.board.AllCoords)
                        {
                            var white = whiteMap.IntensityWithMinMax(coord);
                            var black = blackMap.IntensityWithMinMax(coord);
                            goban.Influence(coord.X, coord.Y, black, white);
                        }
                        /////////////////////////////////////////////////////////////////////
                        if (move != null) { 
                            var single = new BitPlane(game.BoardSize);
                            single.Add(move.Stone);
                            var ImpactMap= new RayTracer(single, solids).GetMap();
                        
                            foreach (var coord in game.board.AllCoords)
                            {
                                var impact = ImpactMap.IntensityWithMinMax(coord);
                                goban.Impact(coord.X, coord.Y, impact);
                            }
                        }

                        /////////////////////////////////////////////////////////////////////

                        goban.SetBlack();
                        foreach (var coord in game.board.White.Unabled)
                        {
                            goban.Stone(coord.X, coord.Y);
                        }

                        goban.SetWhite();
                        foreach (var coord in game.board.Black.Unabled)
                        {
                            goban.Stone(coord.X, coord.Y);
                        }

                        goban.Focus(move.Stone.X, move.Stone.Y);

                        using (var file = encoding.NextFrame()) goban.ReadPng(file);

                        if (lastmove != null)
                        {
                            var dist =
                                (int)
                                    Math.Round(
                                        Math.Sqrt(Math.Pow(lastmove.Stone.X - move.Stone.X, 2) +
                                                  Math.Pow(lastmove.Stone.Y - move.Stone.Y, 2)));
                            c.Tune(main.TonalDegree(dist), m.Current, d.Whole);
                            m.Move();
                        }
                        else
                        {
                            c.Tune(main.Dominante, m.Current, d.Whole);
                            m.Move();
                        }
                        lastmove = move;
                    }

                    c.ToWav(wav);
                }

                encoding.SaveAsMP4Release(env.Output);
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