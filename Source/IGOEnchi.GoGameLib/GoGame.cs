using System;

namespace IGOEnchi.GoGameLogic
{
    public class GoGame
    {
        public Board board;
        public GoNode currentNode;
        private bool updateBoard = true;

        public GoGame(byte boardSize)
        {
            currentNode = new GoRootNode(boardSize);
            MoveNumber = 0;
            Info = new GameInfo();
            ShowVariants = Variants.Siblings;

            board = new Board(boardSize);
        }

        public GameInfo Info { get; set; }

        public int MoveNumber { get; private set; }

        public byte BoardSize
        {
            get { return board.Size; }
        }

        public string CurrentComment
        {
            get { return currentNode.Comment; }
        }

        public GoNode CurrentNode
        {
            get { return currentNode; }
        }

        public GoNode RootNode
        {
            get
            {
                var node = currentNode;
                while (node.ParentNode != null)
                {
                    node = node.ParentNode;
                }
                return node;
            }
        }

        public bool UpdateBoard
        {
            get { return updateBoard; }
            set { updateBoard = value; }
        }

        public Variants ShowVariants { get; set; }

        public int[] Path()
        {
            var path = new int[MoveNumber];
            var node = currentNode;
            var index = MoveNumber - 1;
            while (index >= 0)
            {
                var parent = node.ParentNode;
                path[index] = parent.ChildNodes.IndexOf(node);
                node = parent;
                index -= 1;
            }
            return path;
        }

        public GoGame Copy()
        {
            throw new NotImplementedException();
            /*
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            SGFWriter.SaveGame(this, writer);
            writer.Flush();
            stream.Position = 0;
            var game = SGFCompiler.Compile(SGFLoader.LoadFromStream(stream));
            stream.Close();

            game.ToMove(0);
            foreach (var index in Path())
            {
                game.currentNode = game.currentNode.GetChild(index);
                game.MoveNumber = game.MoveNumber + 1;
            }
            game.Update();

            return game;*/
        }

        public void Update()
        {
            board = currentNode.BoardCopy;
        }

        public bool CanPlace(Stone stone)
        {
            if (board.HasStone(stone) || board.HasStone(stone.OtherColorStone()))
            {
                return false;
            }
            var canPlace = true;
            board.Place(stone);
            if (board.GroupIsDead(stone))
            {
                canPlace = false;
            }
            board.Remove(stone);
            return canPlace;
        }

        public bool BlackToPlay()
        {
            if (CurrentNode is GoSetupNode)
            {
                return (CurrentNode as GoSetupNode).BlackToPlay ?? true;
            }
            return !(CurrentNode as GoMoveNode).Stone.IsBlack;
        }

        public void AddStone(Stone stone)
        {
            GoSetupNode node = null;
            if (currentNode is GoSetupNode)
            {
                node = currentNode as GoSetupNode;
            }
            else
            {
                node = new GoSetupNode(CurrentNode);
                AddNode(node);
            }
            node.RemoveStone(stone);
            node.AddStone(stone);

            board.Remove(stone);
            board.Place(stone, false);
        }

        public void RemoveStone(Stone stone)
        {
            if (currentNode is GoSetupNode)
            {
                (currentNode as GoSetupNode).RemoveStone(stone);
                board.Remove(stone);
            }
            else
            {
                var node = new GoSetupNode(CurrentNode);
                AddNode(node);
                node.RemoveStone(stone);
                board.Remove(stone);
            }
        }

        public bool Continue(Stone stone)
        {
            var nodes = CurrentNode.ChildNodes.FindAll(
                delegate(GoNode node)
                {
                    return node is GoMoveNode &&
                           (node as GoMoveNode).Stone.SameAs(stone);
                });

            if (nodes.Count > 0)
            {
                ToMove(nodes[0]);
                return true;
            }
            return false;
        }

        public void ToMove(GoNode node)
        {
            foreach (var child in currentNode.ChildNodes)
            {
                if (child == node)
                {
                    currentNode = node;
                    MoveNumber += 1;
                    if (updateBoard)
                    {
                        board = currentNode.BoardCopy;
                    }
                }
            }
        }

        public void ToMove(int moveNumber)
        {
            if (MoveNumber > moveNumber)
            {
                while (MoveNumber > moveNumber)
                {
                    ToPreviousMove(false);
                }
            }
            else
            {
                if (MoveNumber < moveNumber)
                {
                    while (MoveNumber < moveNumber)
                    {
                        ToNextMove(false);
                    }
                }
            }
            if (updateBoard)
            {
                board = currentNode.BoardCopy;
            }
        }

        public void AddNode(GoNode node)
        {
            currentNode = currentNode.AddNode(node);
            MoveNumber = MoveNumber + 1;
            if (updateBoard)
            {
                board = currentNode.BoardCopy;
            }
        }

        public void PlaceStone(Stone stone)
        {
            PlaceStone(stone, true);
        }

        public void PlaceStone(Stone stone, bool storeBoard)
        {
            currentNode = currentNode.AddNode(new GoMoveNode(currentNode, stone, storeBoard));
            MoveNumber = MoveNumber + 1;
            if (updateBoard)
            {
                board.Place(stone);
            }
        }

        public void SetHandicap(byte count)
        {
            if (currentNode is GoRootNode)
            {
                currentNode = currentNode.AddNode(new GoSetupNode(currentNode));
                (currentNode as GoSetupNode).SetHandicap(count);
                MoveNumber = MoveNumber + 1;
                board = currentNode.BoardCopy;
            }
        }

        public void CommentNode(string comment)
        {
            currentNode.Comment = comment;
        }

        public bool ToPreviousMove(bool updateBoard)
        {
            if (currentNode.ParentNode != null)
            {
                currentNode = currentNode.ParentNode;
                MoveNumber = MoveNumber - 1;
                if (updateBoard)
                {
                    board = currentNode.BoardCopy;
                }
                return true;
            }
            return false;
        }

        public bool ToPreviousMove()
        {
            return ToPreviousMove(true);
        }

        public bool ToNextMove(bool updateBoard)
        {
            if (currentNode.DefaultChildNode != null)
            {
                currentNode = currentNode.DefaultChildNode;
                MoveNumber = MoveNumber + 1;
                if (updateBoard)
                {
                    if (currentNode is GoMoveNode)
                    {
                        board.Place((currentNode as GoMoveNode).Stone);
                    }
                    else
                    {
                        board = currentNode.BoardCopy;
                    }
                }
                return true;
            }
            return false;
        }

        public bool ToNextMove()
        {
            return ToNextMove(true);
        }

        public void ToPreviousVariation()
        {
            currentNode = currentNode.GetPreviousSibling();
            board = currentNode.BoardCopy;
        }

        public void ToNextVariation()
        {
            currentNode = currentNode.GetNextSibling();
            board = currentNode.BoardCopy;
        }
    }
}