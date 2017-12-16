/*  Copyright (C) 2006-2009 Valentin Kraevskiy
 * 	This file is part of IGoEnchi project
 *	Please read ..\license.txt for conditions of distribution and use
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using IGOEnchi.GoGameLogic;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.GoGameSgf
{

    /// <summary>
    /// Convert SGFTree to GoGame
    /// </summary>
    public class SgfCompiler
    {
        public static CultureInfo CultureInfoDefaultConfiguration = CultureInfo.InvariantCulture;
        public static int BoardStorageStepConfiguration = 20;

        private static bool useRoot;
        private static readonly Regex AntiUnixRegex = new Regex("(?<!\r)\n", RegexOptions.Compiled | RegexOptions.Multiline);
        private static StoneTransformer stoneTransformer;
        private static readonly Random random = new Random(unchecked((int) DateTime.Now.Ticks));
        
        private delegate void StoneTransformer(Stone stone);

        public static IEnumerable<GoGame> Compile(IEnumerable<SGFTree> trees, TransformType transform)
        {
            foreach (var tree in trees)
            {
                yield return Compile(tree, transform);
            }
        }

        public static GoGame Compile(SGFTree gameTree)
        {
            return Compile(gameTree, TransformType.None);
        }

        public static GoGame Compile(SGFTree gameTree, TransformType transform)
        {
            int size = 19;
            if (gameTree.ChildNodes.Count == 0)
            {
                throw new FormatException("No game data");
            }
            if (gameTree.ChildNodes.Count > 1)
            {
                throw new FormatException("Game lists are not supported");
            }

            var rootNode = gameTree.ChildNodes[0];
            foreach (var property in rootNode.Properties)
            {
                if (property.Name == "SZ")
                {
                    size = Convert.ToInt16(property.Value);
                    if (!(size >= 2 && size <= 19))
                    {
                        throw new FormatException("Board size should be between 2 and 19"); 
                    }
                }
            }

            SetTransformer(size, transform);

            var game = new GoGame(size);
            foreach (var property in rootNode.Properties)
            {
                if (property.Name == "BR")
                {
                    game.Info.BlackRank = property.Value;
                }
                if (property.Name == "WR")
                {
                    game.Info.WhiteRank = property.Value;
                }
                if (property.Name == "DT")
                {
                    game.Info.DateOfGame = property.Value;
                }
                if (property.Name == "GC")
                {
                    game.Info.Summarize = property.Value;
                }
                if (property.Name == "RU")
                {
                    game.Info.Rules  = property.Value;
                }
                if (property.Name == "AP")
                {
                    game.Info.ProgramName = property.Value;
                }
                if (property.Name == "GN")
                {
                    game.Info.Name = property.Value;
                }
                if (property.Name == "PW")
                {
                    game.Info.WhitePlayer = property.Value;
                }
                if (property.Name == "PB")
                {
                    game.Info.BlackPlayer = property.Value;
                }
                if (property.Name == "KM")
                {
                    try
                    {
                        game.Info.Komi = float.Parse(property.Value, CultureInfoDefaultConfiguration);
                    }
                    catch (Exception)
                    {
                        game.Info.Komi = 0.5f;
                    }
                }
                if (property.Name == "HA")
                {
                    try
                    {
                        game.Info.Handicap = Convert.ToInt16(property.Value);
                    }
                    catch (Exception)
                    {
                        game.Info.Handicap = 0;
                    }
                }
            }

            game.UpdateBoard = false;
            useRoot = true;
            CompileNode(game, rootNode);
            game.UpdateBoard = true;
            return game;
        }

        private static void SetTransformer(int boardSize, TransformType type)
        {
            switch (type)
            {
                case TransformType.DiagonalFlip:
                    stoneTransformer =
                        delegate(Stone stone)
                        {
                            stone.X =  (stone.X ^ stone.Y);
                            stone.Y =  (stone.X ^ stone.Y);
                            stone.X =  (stone.X ^ stone.Y);
                        };
                    break;
                case TransformType.CodiagonalFlip:
                    stoneTransformer =
                        delegate(Stone stone)
                        {
                            stone.X =  (stone.X ^ stone.Y);
                            stone.Y =  (stone.X ^ stone.Y);
                            stone.X =  (boardSize - (stone.X ^ stone.Y) - 1);
                            stone.Y =  (boardSize - stone.Y - 1);
                        };
                    break;
                case TransformType.HorizontalFlip:
                    stoneTransformer =
                        delegate(Stone stone) { stone.X = (boardSize - stone.X - 1); };
                    break;
                case TransformType.VerticalFlip:
                    stoneTransformer =
                        delegate(Stone stone) { stone.Y = (boardSize - stone.Y - 1); };
                    break;
                case TransformType.Rotate90:
                    stoneTransformer =
                        delegate(Stone stone)
                        {
                            stone.X =  (stone.X ^ stone.Y);
                            stone.Y =  (stone.X ^ stone.Y);
                            stone.X = (boardSize - (stone.X ^ stone.Y) - 1);
                        };
                    break;
                case TransformType.Rotate180:
                    stoneTransformer =
                        delegate(Stone stone)
                        {
                            stone.X = (boardSize - stone.X - 1);
                            stone.Y = (boardSize - stone.Y - 1);
                        };
                    break;
                case TransformType.Rotate270:
                    stoneTransformer =
                        delegate(Stone stone)
                        {
                            stone.Y = (stone.X ^ stone.Y);
                            stone.X = (stone.X ^ stone.Y);
                            stone.Y = (boardSize - (stone.X ^ stone.Y) - 1);
                        };
                    break;
                case TransformType.Random:
                    SetTransformer(boardSize, (TransformType) random.Next(0, 8));
                    break;
                default:
                    stoneTransformer = null;
                    break;
            }
        }
         
        private static void CompileNode(GoGame game, SGFTree tree)
        {
            var singleBranch = true;
            while (singleBranch)
            {
                if (IsMoveNode(tree))
                {
                    var node = new GoMoveNode(game.CurrentNode,
                        FindStone(tree),
                        game.MoveNumber%BoardStorageStepConfiguration == 0);

                    game.AddNode(node);
                }
                else
                {
                    GoSetupNode node;
                    if (game.CurrentNode is GoRootNode && useRoot)
                    {
                        node = game.CurrentNode as GoRootNode;
                    }
                    else
                    {
                        node = new GoSetupNode(game.CurrentNode);
                    }

                    foreach (var property in tree.Properties)
                    {
                        var name = property.Name.ToUpper();
                        switch (name)
                        {
                            case "AB":
                            case "AW":
                                foreach (var stone in CompilePointValues(property.Value))
                                {
                                    stone.IsBlack = name == "AB";
                                    node.AddStone(stone);
                                }
                                break;
                            case "AE":
                                foreach (var stone in CompilePointValues(property.Value))
                                {
                                    node.RemoveStone(stone);
                                }
                                break;
                            case "PL":
                                node.SetPlayer(property.Value.StartsWith("B"));
                                break;
                        }
                    }
                    if (!(game.CurrentNode is GoRootNode && useRoot))
                    {
                        game.AddNode(node);
                    }
                    else
                    {
                        useRoot = false;
                    }
                }
                CompileProperties(game.CurrentNode, tree);

                if (tree.ChildNodes.Count == 1)
                {
                    tree = tree.ChildNodes[0];
                }
                else
                {
                    singleBranch = false;
                }
            }

            var move = game.MoveNumber;
            foreach (var branch in tree.ChildNodes)
            {
                CompileNode(game, branch);
                game.ToMove(move);
            }
        }

        private static void CompileProperties(GoNode node, SGFTree tree)
        {
            foreach (var property in tree.Properties)
            {
                var name = property.Name.ToUpper();
                switch (name)
                {
                    case "C":
                        node.Comment += AntiUnixRegex.Replace(property.Value, "\r\n");
                        break;
                    case "LB":
                        node.EnsureMarkup();
                        node.Markup.Labels.Add(CompileLabelValue(property.Value));
                        break;
                    case "SQ":
                    case "CR":
                    case "TR":
                    case "MA":
                    case "SL":
                        node.EnsureMarkup();
                        var mark =
                            name == "SQ"
                                ? MarkType.Square
                                : name == "CR"
                                    ? MarkType.Circle
                                    : name == "TR"
                                        ? MarkType.Triangle
                                        : name == "MA"
                                            ? MarkType.Mark
                                            : MarkType.Selected;

                        foreach (var stone in CompilePointValues(property.Value))
                        {
                            node.Markup.Marks.Add(new Mark(stone.X, stone.Y, mark));
                        }
                        break;
                }
            }
        }

        private static Stone FindStone(SGFTree tree)
        {
            foreach (var property in tree.Properties)
            {
                var name = property.Name.ToUpper();
                if (name == "B" || name == "W")
                {
                    var result = CompilePointValue(property.Value.ToLower());
                    result.IsBlack = name == "B";
                    return result;
                }
            }
            return new Stone();
        }

        private static TextLabel CompileLabelValue(string source)
        {
            try
            {
                var parts = source.Split(':'); //TODO buggy - Text can be contain ":"
                var stone = CompilePointValue(parts[0]);
                return new TextLabel(stone.X, stone.Y,
                    parts[1].Substring(0, Math.Min(3, parts[1].Length)));
            }
            catch (Exception)
            {
                throw new Exception("Invalid label value: " + source);
            }
        }

        private static Stone CompilePointValue(string source)
        {
            try
            {
                var stone = new Stone();
                source.Trim();
                if (source.Length >= 2)
                {
                    stone.X = Convert.ToInt16(source[0] - 'a');
                    stone.Y = Convert.ToInt16(source[1] - 'a');
                    if (stoneTransformer != null)
                    {
                        stoneTransformer(stone);
                    }
                }
                else
                {
                    stone.X = 20;
                    stone.Y = 20;
                }
                return stone;
            }
            catch (Exception)
            {
                throw new Exception("Invalid point value: " + source);
            }
        }

        private static IEnumerable<Stone> CompilePointValues(string source)
        {
            var parts = source.Split(':');
            if (parts.Length == 1)
            {
                yield return CompilePointValue(parts[0]);
            }
            else if (parts.Length == 2)
            {
                var start = CompilePointValue(parts[0]);
                var end = CompilePointValue(parts[1]);
                var minX = Math.Min(start.X, end.X);
                var maxX = Math.Max(start.X, end.X);
                var minY = Math.Min(start.Y, end.Y);
                var maxY = Math.Max(start.Y, end.Y);

                for (var x = minX; x <= maxX; x++)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        yield return new Stone(x, y, true);
                    }
                }
            }
            else
            {
                throw new Exception("Invalid point value: " + source);
            }
        }

        private static bool IsMoveNode(SGFTree tree)
        {
            foreach (var property in tree.Properties)
            {
                var name = property.Name.ToUpper();
                if (name == "B" || name == "W")
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsSetupNode(SGFTree tree)
        {
            foreach (var property in tree.Properties)
            {
                var name = property.Name.ToUpper();
                if (name == "AD" ||
                    name == "AW" ||
                    name == "AT")
                {
                    return true;
                }
            }
            return false;
        }

    }
}