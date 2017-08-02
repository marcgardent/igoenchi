namespace IGOEnchi.GoGameLogic
{
    public class GoRootNode : GoSetupNode
    {
        public GoRootNode(int boardSize) : base(null)
        {
            board = new Board(boardSize);
        }

        protected override Board CopyParentBoard()
        {
            return board.Copy();
        }
    }
}