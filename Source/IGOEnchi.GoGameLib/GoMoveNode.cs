namespace IGOEnchi.GoGameLogic.Models
{
    public class GoMoveNode : GoNode
    {
        public GoMoveNode(GoNode parentNode, Stone stone, bool makeBoard) : base(parentNode, makeBoard)
        {
            if (board != null)
            {
                board.Place(stone);
            }
            Stone = stone;
        }

        public Stone Stone { get; private set; }

        protected override Board CopyParentBoard()
        {
            var result = base.CopyParentBoard();
            if (Stone != null)
            {
                result.Place(Stone);
            }
            return result;
        }
    }
}