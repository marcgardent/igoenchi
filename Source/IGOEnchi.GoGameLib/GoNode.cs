using System;
using System.Collections.Generic;

namespace IGOEnchi.GoGameLogic.Models
{
    public abstract class GoNode
    {
        protected Board board;

        public GoNode(GoNode parentNode, bool makeBoards)
        {
            ChildNodes = new List<GoNode>();
            if (parentNode != null)
            {
                ParentNode = parentNode;
                if (makeBoards)
                {
                    board = CopyParentBoard();
                }
            }
            else
            {
                ParentNode = null;
            }
        }

        public BoardMarkup Markup { get; private set; }

        public List<GoNode> ChildNodes { get; private set; }

        public GoNode DefaultChildNode { get; private set; }

        public GoNode ParentNode { get; private set; }

        public string Comment { get; set; }

        public Board BoardCopy
        {
            get
            {
                if (board != null)
                {
                    return board.Copy();
                }
                return CopyParentBoard();
            }
        }

        public byte BoardSize
        {
            get
            {
                if (board != null)
                {
                    return board.Size;
                }
                return ParentNode.BoardSize;
            }
        }

        public bool HasChildren
        {
            get { return ChildNodes.Count > 0; }
        }

        protected virtual Board CopyParentBoard()
        {
            if (ParentNode.board != null)
            {
                return ParentNode.board.Copy();
            }
            return ParentNode.CopyParentBoard();
        }

        public GoNode GetNextSibling()
        {
            if (ParentNode == null)
            {
                return this;
            }
            var index = ParentNode.ChildNodes.IndexOf(ParentNode.DefaultChildNode) + 1;
            return ParentNode.GetChild(index);
        }

        public GoNode GetPreviousSibling()
        {
            if (ParentNode == null)
            {
                return this;
            }
            var index = ParentNode.ChildNodes.IndexOf(ParentNode.DefaultChildNode) - 1;
            return ParentNode.GetChild(index);
        }

        public bool Exists(Predicate<GoNode> p)
        {
            if (p(this))
            {
                return true;
            }
            return ChildNodes.Exists(c => Exists(p));
        }

        public GoNode GetChild(int index)
        {
            if (index < 0)
            {
                index = 0;
            }
            if (index >= ChildNodes.Count)
            {
                index = ChildNodes.Count - 1;
            }

            DefaultChildNode = ChildNodes[index];
            return DefaultChildNode;
        }

        public GoNode AddNode(GoNode nextNode)
        {
            ChildNodes.Add(nextNode);
            DefaultChildNode = nextNode;

            return nextNode;
        }

        public void RemoveNode(GoNode node)
        {
            ChildNodes.Remove(node);
            if (node == DefaultChildNode)
            {
                DefaultChildNode =
                    ChildNodes.Count > 0
                        ? ChildNodes[0]
                        : null;
            }
        }

        public void EnsureMarkup()
        {
            if (Markup == null)
            {
                Markup = new BoardMarkup(BoardSize);
            }
        }

        public void MarkDead(Stone stone)
        {
            EnsureMarkup();
            Markup.EnsureDeadStones();

            Markup.DeadStones[stone.X, stone.Y] = true;

            stone.IsBlack = board.Black[stone.X, stone.Y];

            var newStone = stone.LeftStone();
            if (board.HasStone(newStone) && !Markup.DeadStones[newStone.X, newStone.Y])
            {
                MarkDead(newStone);
            }
            newStone = stone.TopStone();
            if (board.HasStone(newStone) && !Markup.DeadStones[newStone.X, newStone.Y])
            {
                MarkDead(newStone);
            }
            newStone = stone.RightStone();
            if (board.HasStone(newStone) && !Markup.DeadStones[newStone.X, newStone.Y])
            {
                MarkDead(newStone);
            }
            newStone = stone.BottomStone();
            if (board.HasStone(newStone) && !Markup.DeadStones[newStone.X, newStone.Y])
            {
                MarkDead(newStone);
            }
        }
    }
}