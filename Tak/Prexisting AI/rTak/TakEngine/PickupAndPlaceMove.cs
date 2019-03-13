using System.Collections.Generic;

namespace TakEngine
{
    /// <summary>
    /// Represents picking up 1 or more pieces from a stack, putting them into our hand, and then placing them in other cells
    /// </summary>
    public class PickupAndPlaceMove : IMove
    {
        /// <summary>
        /// Board position at which pieces are picked up
        /// </summary>
        public BoardPosition Pos;

        /// <summary>
        /// Numbers of stones picked up from the stack
        /// </summary>
        public int PickupCount;

        /// <summary>
        /// 0 = east, 1 = north, etc...
        /// </summary>
        public int UnstackDirection;

        /// <summary>
        /// Number of stones placed at each location when running the stack
        /// </summary>
        public int[] PlaceCounts;

        /// <summary>
        /// indicates if we flattened a standing stone when making the move
        /// </summary>
        private bool _flattened;

        /// <summary>
        /// Stack size prior to picking anything up
        /// </summary>
        private int _stackSize;

        /// <summary>
        /// Create pickup and place move
        /// </summary>
        /// <param name="pos">Board position of the stack being picked up</param>
        /// <param name="pickupCount">Number of stones to take from the initial stack</param>
        /// <param name="stackSize">Stack size prior to picking anything up</param>
        /// <param name="unstackDiration">Direcion in which to unstack (0 = east, 1 = north, etc...)</param>
        /// <param name="placeCounts">Number of stones to unstack in each location</param>
        public PickupAndPlaceMove(BoardPosition pos, int pickupCount, int stackSize, int unstackDiration, params int[] placeCounts)
        {
            Pos = pos;
            _stackSize = stackSize;
            PickupCount = pickupCount;
            PlaceCounts = placeCounts;
            UnstackDirection = unstackDiration;
        }

        string _notated;
        public string Notate()
        {
            if (_notated == null)
            {
                var sb = new System.Text.StringBuilder();
                if (PickupCount > 1 || _stackSize != PickupCount)
                    sb.Append(PickupCount);
                sb.Append(Pos.Describe());
                sb.Append(Direction.DirName[UnstackDirection]);
                if (PlaceCounts.Length > 1)
                {
                    for (int i = 0; i < PlaceCounts.Length; i++)
                        sb.Append(PlaceCounts[i]);
                }
                _notated = sb.ToString();
            }
            return _notated;
        }

        public override string ToString()
        {
            return Notate();
        }

        public void MakeMove(GameState game)
        {
            var pickupstack = game.Board[Pos.X, Pos.Y];
            int dx = Direction.DirX[UnstackDirection];
            int dy = Direction.DirY[UnstackDirection];
            int unstackcount = 0;
            for (int i = 0; i < PlaceCounts.Length; i++)
            {
                var target = new BoardPosition(Pos.X + dx * (i + 1), Pos.Y + dy * (i + 1));
                var targetstack = game.Board[target.X, target.Y];

                if (targetstack.Count > 0 && Piece.GetStone(targetstack[targetstack.Count - 1]) == Piece.Stone_Standing)
                {
                    var player = Piece.GetPlayerID(targetstack[targetstack.Count - 1]);
                    targetstack[targetstack.Count - 1] = Piece.MakePieceID(Piece.Stone_Flat, player);
                    _flattened = true;
                }
                for (int j = 0; j < PlaceCounts[i]; j++)
                {
                    targetstack.Add(pickupstack[pickupstack.Count - PickupCount + unstackcount]);
                    unstackcount++;
                }
            }

            for (int i = 0; i < PickupCount; i++)
                pickupstack.RemoveAt(pickupstack.Count - 1);
        }

        public void TakeBackMove(GameState game)
        {
            var stack = game.Board[Pos.X, Pos.Y];
            int dx = Direction.DirX[UnstackDirection];
            int dy = Direction.DirY[UnstackDirection];
            for (int i = 0; i < PlaceCounts.Length; i++)
            {
                var target = new BoardPosition(Pos.X + dx * (i + 1), Pos.Y + dy * (i + 1));
                var targetstack = game.Board[target.X, target.Y];

                int pci = PlaceCounts[i];
                for (int j = 0; j < pci; j++)
                    stack.Add(targetstack[targetstack.Count - pci + j]);
                for (int j = 0; j < pci; j++)
                    targetstack.RemoveAt(targetstack.Count - 1);
                if (_flattened && i == PlaceCounts.Length - 1)
                {
                    var player = Piece.GetPlayerID(targetstack[targetstack.Count - 1]);
                    targetstack[targetstack.Count - 1] = Piece.MakePieceID(Piece.Stone_Standing, player);
                }
            }
        }

        public bool CompareTo(IMove move)
        {
            var cmp = move as PickupAndPlaceMove;
            if (cmp != null)
            {
                if (cmp.Pos == Pos &&
                    cmp.PickupCount == PickupCount &&
                    cmp.UnstackDirection == UnstackDirection &&
                    cmp.PlaceCounts.Length == PlaceCounts.Length)
                {
                    for (int i = 0; i < PlaceCounts.Length; i++)
                        if (cmp.PlaceCounts[i] != PlaceCounts[i])
                            return false;
                    return true;
                }
                return false;
            }
            else
                return false;
        }
    }
}
