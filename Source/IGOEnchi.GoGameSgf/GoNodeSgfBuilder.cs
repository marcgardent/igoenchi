using System;
using IGOEnchi.GoGameLogic;
using IGOEnchi.SmartGameLib;
using IGOEnchi.SmartGameLib.models;

namespace IGOEnchi.GoGameSgf
{
    public class GoNodeSgfBuilder
    {
        private GoNode goNode;
        private SgfBuilder b;
        
        public GoNodeSgfBuilder(GoNode goNode, SgfBuilder b)
        {
            this.goNode = goNode;
            this.b = b;
        }

        public GoNodeSgfBuilder(GoNode goNode) : this(goNode,new SgfBuilder())
        {
        }

        public SGFTree ToSGFTree()
        {
            BuildNode();
            return b.ToSgfTree();
        }

        public void BuidInfoGame(GoGame game)
        {
            b.Root()
                .p("GM", "1")
                .p("FF", "4")
                .p("CA", "UTF-8")
                .p("SZ", game.BoardSize)
                .p("KM", game.Info.Komi)
                .p("PW", game.Info.WhitePlayer)
                .p("PB", game.Info.BlackPlayer);

            if (game.Info.Handicap > 1) b.p("HA", game.Info.Handicap);
        }

        private void BuildNode()
        {
            var node = goNode;
            while (node != null)
            {
                if (node is GoMoveNode)
                {
                    var currentNode = node as GoMoveNode;
                    GoMove(currentNode.Stone);
                }

                else if (node is GoSetupNode)
                {
                    BuildGoSetupNode(node);
                }

                if (node.Comment != null)
                {
                    b.Text("C", node.Comment);
                }

                if (node.Markup != null)
                {
                    BuildMarkup(node);
                }

                if (node.ChildNodes.Count == 1)
                {
                    node = node.ChildNodes[0];
                    b.Next();
                }
                else
                {
                    foreach (var child in node.ChildNodes)
                    {
                        b.Fork(x =>
                        
                            new GoNodeSgfBuilder(child, x).ToSGFTree()
                        );
                    }
                    node = null;
                }
            }
        }

        private void BuildMarkup(GoNode node)
        {
            foreach (var label in node.Markup.Labels)
            {
                b.Compose("LB", DecompilePointValue(label.X, label.Y), label.Text);
            }

            foreach (var mark in node.Markup.Marks)
            {
                var coord = DecompilePointValue(mark.X, mark.Y);
                string markTypeToken = "SQ";
                switch (mark.MarkType)
                {
                    case MarkType.Square:
                        markTypeToken = "SQ";
                        break;
                    case MarkType.Circle:
                        markTypeToken = "CR";
                        break;
                    case MarkType.Triangle:
                        markTypeToken = "TR";
                        break;
                    case MarkType.Mark:
                        markTypeToken = "MA";
                        break;
                    case MarkType.Selected:
                        markTypeToken = "SL";
                        break;
                }
                b.p(markTypeToken, coord);
            }
        }

        private void BuildGoSetupNode(GoNode node)
        {
            var currentNode = node as GoSetupNode;
            BitPlane stones;
            var currentBoard = currentNode.BoardCopy;
            Board lastBoard = null;
            if (currentNode.ParentNode != null)
            {
                lastBoard = currentNode.ParentNode.BoardCopy;
            }


            if (lastBoard == null)
            {
                stones = currentBoard.Black;
            }
            else
            {
                stones =
                    currentBoard.Black - lastBoard.Black;
            }
            if (!stones.Empty())
            {
                DecompilePlaneValue("AB", stones);
            }
            if (lastBoard == null)
            {
                stones = currentBoard.White;
            }
            else
            {
                stones =
                    currentBoard.White - lastBoard.White;
            }
            if (!stones.Empty())
            {
                DecompilePlaneValue("AW", stones);
            }
            if (lastBoard != null)
            {
                stones = (lastBoard.White - currentBoard.White).And(
                    lastBoard.Black - currentBoard.Black);
                if (!stones.Empty())
                {
                    DecompilePlaneValue("AE", stones);
                }
            }
            if (currentNode.BlackToPlay.HasValue)
            {
                b.p("PL", currentNode.BlackToPlay.Value ? "B" : "W");
            }
        }

        private void DecompilePlaneValue(string key, BitPlane plane)
        {
            
            for (int i = 0; i < plane.Width; i++)
            {
                for (int j = 0; j < plane.Height; j++)
                {
                    if (plane[i, j])
                    {
                        b.p(key, DecompilePointValue(i,j));
                    }
                }
            }
        }

        private void GoMove(Stone stone)
        {
            var coords = DecompilePointValue(stone.X, stone.Y);
            b.p(stone.IsBlack ? "B" : "W", coords);
        }

        private string DecompilePointValue(int x, int y)
        {
            if (x < goNode.BoardSize && y < goNode.BoardSize)
            {
                return Convert.ToChar(x + 'a') + Convert.ToChar(y + 'a').ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}