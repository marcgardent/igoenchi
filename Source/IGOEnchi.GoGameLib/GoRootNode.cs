namespace IGOEnchi.GoGameLogic
{
    public class GoRootNode : GoSetupNode
    {
        public GoRootNode(byte boardSize) : base(null)
        {
            board = new Board(boardSize);
        }

        protected override Board CopyParentBoard()
        {
            return board.Copy();
        }
    }
}