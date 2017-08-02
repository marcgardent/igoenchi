namespace IGOEnchi.GoGameLogic
{
    public class GoSetupNode : GoNode
    {
        public GoSetupNode(GoNode parentNode) : base(parentNode, true)
        {
        }

        public bool? BlackToPlay { get; set; }

        public void SetPlayer(bool black)
        {
            BlackToPlay = black;
        }

        public void SetHandicap(int count)
        {
            switch (board.Size)
            {
                case 19:
                    SetHandicapFor19(count);
                    break;
                case 13:
                    SetHandicapFor13(count);
                    break;
                case 9:
                    SetHandicapFor9(count);
                    break;
            }
        }

        public void SetHandicapFor19(int count)
        {
            switch (count)
            {
                case 2:
                    AddStone(new Stone(3, 15, true));
                    AddStone(new Stone(15, 3, true));
                    break;
                case 3:
                    SetHandicapFor19(2);
                    AddStone(new Stone(3, 3, true));
                    break;
                case 4:
                    SetHandicapFor19(3);
                    AddStone(new Stone(15, 15, true));
                    break;
                case 5:
                    SetHandicapFor19(4);
                    AddStone(new Stone(9, 9, true));
                    break;
                case 6:
                    SetHandicapFor19(4);
                    AddStone(new Stone(3, 9, true));
                    AddStone(new Stone(15, 9, true));
                    break;
                case 7:
                    SetHandicapFor19(6);
                    AddStone(new Stone(9, 9, true));
                    break;
                case 8:
                    SetHandicapFor19(6);
                    AddStone(new Stone(9, 15, true));
                    AddStone(new Stone(9, 3, true));
                    break;
                case 9:
                    SetHandicapFor19(8);
                    AddStone(new Stone(9, 9, true));
                    break;
            }
        }

        public void SetHandicapFor13(int count)
        {
            switch (count)
            {
                case 2:
                    AddStone(new Stone(3, 9, true));
                    AddStone(new Stone(9, 3, true));
                    break;
                case 3:
                    SetHandicapFor13(2);
                    AddStone(new Stone(3, 3, true));
                    break;
                case 4:
                    SetHandicapFor13(3);
                    AddStone(new Stone(9, 9, true));
                    break;
                case 5:
                    SetHandicapFor13(4);
                    AddStone(new Stone(6, 6, true));
                    break;
                case 6:
                    SetHandicapFor13(4);
                    AddStone(new Stone(3, 6, true));
                    AddStone(new Stone(9, 6, true));
                    break;
                case 7:
                    SetHandicapFor13(6);
                    AddStone(new Stone(6, 6, true));
                    break;
                case 8:
                    SetHandicapFor13(6);
                    AddStone(new Stone(6, 9, true));
                    AddStone(new Stone(6, 3, true));
                    break;
                case 9:
                    SetHandicapFor13(8);
                    AddStone(new Stone(6, 6, true));
                    break;
            }
        }

        public void SetHandicapFor9(int count)
        {
            switch (count)
            {
                case 2:
                    AddStone(new Stone(2, 6, true));
                    AddStone(new Stone(6, 2, true));
                    break;
                case 3:
                    SetHandicapFor9(2);
                    AddStone(new Stone(2, 2, true));
                    break;
                case 4:
                    SetHandicapFor9(3);
                    AddStone(new Stone(6, 6, true));
                    break;
                case 5:
                    SetHandicapFor9(4);
                    AddStone(new Stone(4, 4, true));
                    break;
                case 6:
                    SetHandicapFor9(4);
                    AddStone(new Stone(2, 4, true));
                    AddStone(new Stone(6, 4, true));
                    break;
                case 7:
                    SetHandicapFor9(6);
                    AddStone(new Stone(4, 4, true));
                    break;
                case 8:
                    SetHandicapFor9(6);
                    AddStone(new Stone(4, 6, true));
                    AddStone(new Stone(4, 2, true));
                    break;
                case 9:
                    SetHandicapFor9(8);
                    AddStone(new Stone(4, 4, true));
                    break;
            }
        }

        public void AddStone(Stone stone)
        {
            board.Place(stone, false);
        }

        public void RemoveStone(Stone stone)
        {
            board.Remove(stone);
        }
    }
}