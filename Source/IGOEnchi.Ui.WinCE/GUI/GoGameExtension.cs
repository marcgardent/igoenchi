

using IGOEnchi.GoGameLib.Models;
using IGOEnchi.GoGameLogic.Models;

namespace IGoEnchi
{
    public static class GoGameExtension
    {


        public static  void Render(this GoGame goGame,  GameRenderer renderer)
        {
            renderer.ClearBoard(goGame.board.Size);

            for (byte x = 0; x < goGame.board.Size; x++)
            {
                for (byte y = 0; y < goGame.board.Size; y++)
                {
                    if (goGame.board.Black[x, y])
                    {
                        renderer.DrawStone(x, y, true);
                    }
                    else if (goGame.board.White[x, y])
                    {
                        renderer.DrawStone(x, y, false);
                    }
                }
            }

            if (goGame.currentNode is GoMoveNode)
            {
                var stone = (goGame.currentNode as GoMoveNode).Stone;

                renderer.DrawMoveMark(stone.X, stone.Y, goGame.board.HasStone(stone.X, stone.Y, false));
                if (goGame.showVariants == Variants.Siblings)
                {
                    goGame.currentNode.ParentNode.ChildNodes.
                        ForEach(n => RenderShadow(renderer, n));
                }
            }

            if (goGame.showVariants == Variants.Children)
            {
                goGame.currentNode.ChildNodes.
                    ForEach(n => RenderShadow(renderer, n));
            }

            if (goGame.currentNode.Markup != null)
            {
                RenderMarkup(renderer, goGame.currentNode.Markup);
            }

            if (goGame.currentNode is GoMoveNode)
            {
                var stone = (goGame.currentNode as GoMoveNode).Stone;
                if (stone.X > 19)
                {
                    renderer.DrawMessage((stone.IsBlack ? "Black" : "White")
                                         + " passes");
                }
            }
        }

        private static void RenderShadow(this GoGame goGame, GameRenderer renderer, GoNode node)
        {
            if ((node != goGame.currentNode) && (node is GoMoveNode))
            {
                var nodeStone = (node as GoMoveNode).Stone;
                renderer.DrawShadow(nodeStone.X, nodeStone.Y, nodeStone.IsBlack);
            }
        }

        private static void RenderMarkup(this GoGame goGame, GameRenderer renderer, BoardMarkup markup)
        {
            foreach (TextLabel label in markup.Labels)
            {
                renderer.DrawString(label.X, label.Y, label.Text,
                                    goGame.board.HasStone(label.X, label.Y, false));
            }

            foreach (Mark mark in markup.Marks)
            {
                switch (mark.MarkType)
                {
                    case MarkType.Square:
                        renderer.DrawSquare(mark.X, mark.Y, goGame.board.HasStone(mark.X, mark.Y, false));
                        break;
                    case MarkType.Circle:
                        renderer.DrawCircle(mark.X, mark.Y, goGame.board.HasStone(mark.X, mark.Y, false));
                        break;
                    case MarkType.Triangle:
                        renderer.DrawTriangle(mark.X, mark.Y, goGame.board.HasStone(mark.X, mark.Y, false));
                        break;
                    case MarkType.Mark:
                        renderer.DrawMark(mark.X, mark.Y, goGame.board.HasStone(mark.X, mark.Y, false));
                        break;
                    case MarkType.Selected:
                        renderer.DrawSelected(mark.X, mark.Y, goGame.board.HasStone(mark.X, mark.Y, false));
                        break;
                }
            }

            if (markup.Territory != null)
            {
                for (byte x = 0; x < markup.Territory.Black.Width; x++)
                {
                    for (byte y = 0; y < markup.Territory.Black.Width; y++)
                    {
                        if (markup.Territory.Black[x, y])
                        {
                            renderer.DrawTerritory(x, y, true);
                        }
                    }
                }

                for (byte x = 0; x < markup.Territory.White.Width; x++)
                {
                    for (byte y = 0; y < markup.Territory.White.Width; y++)
                    {
                        if (markup.Territory.White[x, y])
                        {
                            renderer.DrawTerritory(x, y, false);
                        }
                    }
                }
            }

            if (markup.DeadStones != null)
            {
                for (byte x = 0; x < markup.DeadStones.Width; x++)
                {
                    for (byte y = 0; y < markup.DeadStones.Width; y++)
                    {
                        if (markup.DeadStones[x, y])
                        {
                            renderer.DrawMark(x, y, goGame.board.HasStone(x, y, false));
                        }
                    }
                }
            }
        } 
    }
}