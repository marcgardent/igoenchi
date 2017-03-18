using System;
using System.Collections.Generic;
using System.IO;
using IGOEnchi.GoGameLogic;
using IGOEnchi.GoGameSgf;
using IGOEnchi.SmartGameLib;
using Io.CNTKTextFormat;

namespace IGOPhoenix.SupervisedLearning
{
    class Program
    {




        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("excepted : SupervisedLearning.exe  \"<path of .sgf folder>\"");
                return;
            }

            var path = args[0];
            var target= new DirectoryInfo(path);

            var dfile = Path.Combine(target.Parent.FullName, target.Name + ".small.sparse.ctf");
            var count = 0;
            using (var txt = CntkTextFormatWriter.CreateCtf(dfile))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.sgf"))
                {
                    Console.WriteLine(file);
                    Console.WriteLine(count);

                    var gogame = OpenFile(file);

                    if (gogame != null)
                    {
                        gogame.ToStart();
                        var serie = txt.Serie();

                        while (true)
                        {
                            var input = gogame.CurrentNode.BoardCopy;
                            if (gogame.ToNextMove())
                            {
                                var currentNode = gogame.CurrentNode as GoMoveNode;
                                if (currentNode != null)
                                {
                                    var output = currentNode.Stone;

                                    var stateHandler = currentNode.Stone.IsBlack
                                        ? new StateHandler(Stateblack)
                                        : new StateHandler(Statewhite);

                                    serie.Sequence()
                                        .Sparse("g", WalkGrid(input, (b, x, y) => stateHandler(b, x, y)))
                                        .Sparse("n",
                                            WalkGrid(input, (b, x, y) => x == output.X && y == output.Y ? 1 : 0))
                                        .CloseSequence();

                                    if (count > 10000)
                                    {
                                        serie.CloseSerie();
                                        break;
                                    }
                                    else
                                    {
                                        count++;
                                    }
                                }
                            }
                            else
                            {
                                serie.CloseSerie();
                                break;
                            }
                        }

                        if (count > 10000)
                        {
                            break;
                        }
                        else
                        {
                            count++;
                        }
                    }

                }
            }
        }

        private static IEnumerable<float> WalkGrid(Board board, StateHandler func )
        {
            for (byte x = 0; x < board.Size; x++)
                for (byte y = 0; y < board.Size; y++)
                {
                    yield return func(board,x, y);
                }
        }

        delegate float StateHandler(Board board, byte x, byte y);

        public  static float Statewhite(Board board, byte x, byte y)
        {

            if (board.StoneInBounds(x, y))
            {
                if (board.HasBlack(x, y))
                {
                    return -1;
                }
                else if (board.HasWhite(x, y))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            return -10;
        }
        public static float Stateblack(Board board, byte x, byte y)
        {

            if (board.StoneInBounds(x, y))
            {
                if (board.HasBlack(x, y))
                {
                    return 1;
                }
                else if (board.HasWhite(x, y))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            return -1;
        }

        private static GoGame OpenFile(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var excepted = SgfReader.LoadFromStream(stream);
                    return SgfCompiler.Compile(excepted);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(path);
                Console.Error.WriteLine(e);
                return null;
            }
        }
    }
}
