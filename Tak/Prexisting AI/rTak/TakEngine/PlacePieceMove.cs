namespace TakEngine
{
    /// <summary>
    /// Move which places the specified piece on the top of the stack at the given board coordinates
    /// </summary>
    public class PlacePieceMove : IMove
    {
        /// <summary>
        /// Piece to place on top of the stack
        /// </summary>
        public int PieceID;

        /// <summary>
        /// Board position at which to place the piece
        /// </summary>
        public BoardPosition Pos;

        /// <summary>
        /// Create move for placing a stone on top of a stack
        /// </summary>
        /// <param name="piece">PieceID of the stone being placed</param>
        /// <param name="pos">Position at which the stone will be placed</param>
        /// <param name="fromReserve">True if the stone is being played from the player's reserve</param>
        /// <param name="flatten">True if this placement action is going to flatten a standing stone</param>
        public PlacePieceMove(int piece, BoardPosition pos)
        {
            PieceID = piece;
            Pos = pos;
        }

        public void MakeMove(GameState game)
        {
            var stack = game.Board[Pos.X, Pos.Y];
            game.Board[Pos.X, Pos.Y].Add(PieceID);
            var stone = Piece.GetStone(PieceID);
            var player = Piece.GetPlayerID(PieceID);
            if (stone == Piece.Stone_Cap)
                game.CapRemaining[player]--;
            else
                game.StonesRemaining[player]--;
        }

        public void TakeBackMove(GameState game)
        {
            var stack = game.Board[Pos.X, Pos.Y];
            stack.RemoveAt(stack.Count - 1);
            var stone = Piece.GetStone(PieceID);
            var player = Piece.GetPlayerID(PieceID);
            if (stone == Piece.Stone_Cap)
                game.CapRemaining[player]++;
            else
                game.StonesRemaining[player]++;
        }

        string _notated;
        public string Notate()
        {
            if (_notated == null)
            {
                _notated = string.Concat(Piece.Describe(PieceID), Pos.Describe());
            }
            return _notated;
        }

        public override string ToString()
        {
            return Notate();
        }

        public bool CompareTo(IMove move)
        {
            var cmp = move as PlacePieceMove;
            if (cmp != null)
                return cmp.PieceID == PieceID && cmp.Pos == Pos;
            else
                return false;
        }
    }
}
