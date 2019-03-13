using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakEngine;

namespace TakGame_WinForms
{
    class InteractiveMove_PickupAndPlace : IBoardInteraction
    {
        GameState _game;
        BoardView _boardView;
        InterPickupAndPlaceMove _move = null;
        InterPlacePieceMove _previewPlace = null;
        PickUpMove _previewPickup = null;

        public InteractiveMove_PickupAndPlace(GameState game, BoardView boardView)
        {
            _game = game;
            _boardView = boardView;
            _boardView.CarryClear();
            _boardView.CarryVisible = false;
        }

        public bool Completed
        {
            get
            {
                if (_move != null)
                {
                    int pickupCount = _move.PickUpMove.PickUpCount;
                    int placeCount = _move.Count - 1 + (_previewPlace != null ? 1 : 0);
                    return pickupCount == placeCount;
                }
                return false;
            }
        }
        public bool HasPreview { get { return _previewPickup != null || _previewPlace != null; } }
        public IMove GetMove() { return _move; }

        public void AcceptPreview()
        {
            _boardView.ClearHighlights();
            if (_previewPickup != null)
            {
                if (_move != null)
                {
                    int oldCount = _move.PickUpMove.PickUpCount;
                    _move.TakeBackMove(_game);
                    _previewPickup = new PickUpMove(
                        _previewPickup.Position,
                        oldCount + _previewPickup.PickUpCount,
                        _game);
                    _boardView.CarryClear();
                }
                _move = new InterPickupAndPlaceMove(_previewPickup);
                _move.MakeMove(_game);
                _boardView.CarryVisible = true;
                foreach (var pieceID in _move.PickUpMove.PickUpPieces)
                    _boardView.CarryAdd(pieceID);
                _previewPickup = null;
            }
            else if (_previewPlace != null)
            {
                _move.AddToChain(_previewPlace);
                _previewPlace = null;
            }
        }

        public void Cancel()
        {
            CancelPreview();
            if (_move != null)
            {
                _move.TakeBackMove(_game);
                _boardView.InvalidateRender();
                _move = null;
            }
            _boardView.CarryClear();
        }

        public void CancelPreview()
        {
            if (_previewPlace != null)
            {
                _previewPlace.TakeBackMove(_game);
                _boardView.InvalidateRender();
                _boardView.CarryInsert(_previewPlace.PieceID);
                _previewPlace = null;
            }
            // NOTE: do not need to undo pick move as it doesn't really get applied until AcceptPreview
            _boardView.ClearHighlights();
        }

        public bool TryPreviewAt(BoardStackPosition mouseOver)
        {
            if (_game.Ply < 2)
                return false;
            _boardView.CarryVisible = true;
            var mouseOverPos = mouseOver.Position;
            if ((_move == null || (_move.PickUpMove.Position == mouseOver.Position && !_move.UnstackDirection.HasValue))
                && mouseOver.StackPos.HasValue)
            {
                int carryCount = 0;
                if (_move != null)
                    carryCount = _move.PickUpMove.PickUpCount;
                var stack = _game.Board[mouseOverPos.X, mouseOverPos.Y];
                if (stack.Count == 0)
                    return false;
                if (carryCount == 0 && Piece.GetPlayerID(stack[stack.Count - 1]) != (_game.Ply & 1))
                    return false;
                int pickUpCount = stack.Count - mouseOver.StackPos.Value;
                if (pickUpCount + carryCount <= _game.Size)
                {
                    _previewPickup = new PickUpMove(mouseOverPos, stack.Count - mouseOver.StackPos.Value, _game);
                    _boardView.SetHighlight(mouseOverPos, mouseOver.StackPos);
                    return true;
                }
            }
            else
            {
                // check for placing pieces
                if (_move == null)
                    return false;
                var placingPiece = _move.PickUpMove.PickUpPieces[_move.Count - 1];
                var covering = _game[mouseOverPos];
                bool flatten = false;
                if (covering.HasValue)
                {
                    // cannot put piece on top of cap stone
                    if (Piece.GetStone(covering.Value) == Piece.Stone_Cap)
                        return false;

                    // standing stone only gets flattened by cap stone
                    if (Piece.GetStone(covering.Value) == Piece.Stone_Standing)
                    {
                        if (Piece.GetStone(placingPiece) != Piece.Stone_Cap)
                            return false;
                        else
                            flatten = true;
                    }
                }

                // validate direction
                var unstackDir = _move.UnstackDirection;
                if (!unstackDir.HasValue)
                {
                    var delta = mouseOverPos - _move.PickUpMove.Position;
                    if (Math.Abs(delta.X) + Math.Abs(delta.Y) != 1)
                        return false;
                }
                else
                {
                    var lastPlacedAt = ((InterPlacePieceMove)_move[_move.Count - 1]).Pos;
                    var delta = mouseOverPos - lastPlacedAt;
                    if (delta != BoardPosition.Zero &&
                        mouseOverPos != Direction.Offset(lastPlacedAt, unstackDir.Value))
                        return false;
                }

                _previewPickup = null;
                _previewPlace = new InterPlacePieceMove(placingPiece, mouseOverPos, false, flatten);
                _previewPlace.MakeMove(_game);
                _boardView.CarryRemoveBottom();
                _boardView.SetHighlight(mouseOverPos, _game.Board[mouseOverPos.X, mouseOverPos.Y].Count - 1);
                _boardView.InvalidateRender();
                _boardView.CarryVisible = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// IMove implementation for picking up stones from the top of a stack and putting them in our hand.
        /// This is NOT a completely legal move in the game, but this object is part of the aggregate PickUpAndPlaceMove
        /// </summary>
        public class PickUpMove : IMove
        {
            /// <summary>
            /// Board position at which pieces will be picked up
            /// </summary>
            BoardPosition _pos;

            /// <summary>
            /// Array of pieces in our hand after picking up pieces.
            /// Element 0 is the bottom, and should be placed first when dropping pieces in adjacent cells
            /// </summary>
            int[] _pickupPieces;

            public readonly int Remaining;

            /// <summary>
            /// Creates the PickUpMove and also inspects the game state to determine which pieces will be picked up
            /// </summary>
            /// <param name="pos">Stack position</param>
            /// <param name="pickUpCount">Number of pieces to pick up from the top of the stack</param>
            /// <param name="game">Game state</param>
            public PickUpMove(BoardPosition pos, int pickUpCount, GameState game)
            {
                _pos = pos;
                _pickupPieces = new int[pickUpCount];
                var stack = game.Board[_pos.X, _pos.Y];
                for (int i = 0; i < _pickupPieces.Length; i++)
                    _pickupPieces[i] = stack[stack.Count - _pickupPieces.Length + i];
                Remaining = stack.Count - pickUpCount;
            }

            public int PickUpCount { get { return _pickupPieces.Length; } }

            /// <summary>
            /// Gets the list of pieces to be picked up.  Element 0 is the bottom which should be placed first when dropping pieces.
            /// </summary>
            public IReadOnlyList<int> PickUpPieces { get { return _pickupPieces; } }

            /// <summary>
            /// Gets whether the stone at the specified position in the hand is a cap stone
            /// </summary>
            /// <param name="offset"></param>
            /// <returns></returns>
            public bool IsCapStone(int offset) { return Piece.GetStone(_pickupPieces[offset]) == Piece.Stone_Cap; }

            public void MakeMove(GameState game)
            {
                var stack = game.Board[_pos.X, _pos.Y];
                for (int i = 0; i < _pickupPieces.Length; i++)
                    stack.RemoveAt(stack.Count - 1);
            }

            public void TakeBackMove(GameState game)
            {
                var stack = game.Board[_pos.X, _pos.Y];
                for (int i = 0; i < _pickupPieces.Length; i++)
                    stack.Add(_pickupPieces[i]);
            }

            /// <summary>
            /// Create a PlacePieceMove for the piece in the specified position of our hand
            /// </summary>
            /// <param name="offset">Hand position, 0 = bottom which is placed first</param>
            /// <param name="pos">Board position at which the piece will be placed</param>
            /// <param name="flatten">True if the placement action will result in flattening a standing stone</param>
            /// <returns></returns>
            public InterPlacePieceMove GeneratePlaceFromStack(int offset, BoardPosition pos, bool flatten)
            {
                return new InterPlacePieceMove(_pickupPieces[offset], pos, false, flatten);
            }
            public BoardPosition Position { get { return _pos; } }

            /// <summary>
            /// INVALID OPERATION because this isn't a legal move by itself
            /// </summary>
            public string Notate()
            {
                throw new InvalidOperationException();
            }

            public bool CompareTo(IMove move)
            {
                var cmp = move as PickUpMove;
                if (cmp != null)
                {
                    if (cmp._pos != _pos ||
                        cmp._pickupPieces.Length != _pickupPieces.Length)
                        return false;
                    for (int i = 0; i < _pickupPieces.Length; i++)
                        if (_pickupPieces[i] != cmp._pickupPieces[i])
                            return false;
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Abstract class for representing a move as a series of moves that are performed in sequence (and taken back in reverse order)
        /// </summary>
        public abstract class ChainMove : IMove
        {
            protected List<IMove> Moves = new List<IMove>(4);

            public ChainMove()
            {
            }

            public ChainMove(IEnumerable<IMove> moves)
            {
                Moves.AddRange(moves);
            }

            public void AddToChain(IMove move)
            {
                Moves.Add(move);
            }

            /// <summary>
            /// Perform the contained moves in order
            /// </summary>
            /// <param name="game"></param>
            public void MakeMove(GameState game)
            {
                for (int i = 0; i < Moves.Count; i++)
                    Moves[i].MakeMove(game);
            }

            /// <summary>
            /// Take back the contained moves in reverse order
            /// </summary>
            /// <param name="game"></param>
            public void TakeBackMove(GameState game)
            {
                for (int i = Moves.Count - 1; i >= 0; i--)
                    Moves[i].TakeBackMove(game);
            }
            public int Count { get { return Moves.Count; } }
            public IMove this[int index] { get { return Moves[index]; } }

            public void RemoveFromEnd(int count)
            {
                Moves.RemoveRange(Moves.Count - count, count);
            }

            /// <summary>
            /// Notation is only exists for full-defined moves, so this must be marked abstract
            /// </summary>
            /// <returns></returns>
            public abstract string Notate();

            public bool CompareTo(IMove move)
            {
                var cmp = move as ChainMove;
                if (cmp != null)
                {
                    for (int i = 0; i < cmp.Moves.Count; i++)
                        if (!cmp.Moves[i].CompareTo(Moves[i]))
                            return false;
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Move which places the specified piece on the top of the stack at the given board coordinates
        /// </summary>
        public class InterPlacePieceMove : IMove
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
            /// True if this stone is being placed from the player's reserve
            /// </summary>
            public bool FromReserve;

            /// <summary>
            /// True if this placement action is going to flatten a standing stone
            /// </summary>
            public bool Flatten;

            /// <summary>
            /// Create move for placing a stone on top of a stack
            /// </summary>
            /// <param name="piece">PieceID of the stone being placed</param>
            /// <param name="pos">Position at which the stone will be placed</param>
            /// <param name="fromReserve">True if the stone is being played from the player's reserve</param>
            /// <param name="flatten">True if this placement action is going to flatten a standing stone</param>
            public InterPlacePieceMove(int piece, BoardPosition pos, bool fromReserve, bool flatten)
            {
                PieceID = piece;
                Pos = pos;
                FromReserve = fromReserve;
                Flatten = flatten;
            }

            public void MakeMove(GameState game)
            {
                var stack = game.Board[Pos.X, Pos.Y];
                if (Flatten)
                    stack[stack.Count - 1] = Piece.MakePieceID(Piece.Stone_Flat, Piece.GetPlayerID(stack[stack.Count - 1]));
                game.Board[Pos.X, Pos.Y].Add(PieceID);
                var stone = Piece.GetStone(PieceID);
                var player = Piece.GetPlayerID(PieceID);
                if (FromReserve)
                {
                    if (stone == Piece.Stone_Cap)
                        game.CapRemaining[player]--;
                    else
                        game.StonesRemaining[player]--;
                }
            }

            public void TakeBackMove(GameState game)
            {
                var stack = game.Board[Pos.X, Pos.Y];
                stack.RemoveAt(stack.Count - 1);
                var stone = Piece.GetStone(PieceID);
                var player = Piece.GetPlayerID(PieceID);
                if (FromReserve)
                {
                    if (stone == Piece.Stone_Cap)
                        game.CapRemaining[player]++;
                    else
                        game.StonesRemaining[player]++;
                }
                if (Flatten)
                    stack[stack.Count - 1] = Piece.MakePieceID(Piece.Stone_Standing, Piece.GetPlayerID(stack[stack.Count - 1]));
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

        /// <summary>
        /// Represents picking up 1 or more pieces from a stack, putting them into our hand, and then placing them in other cells
        /// </summary>
        public class InterPickupAndPlaceMove : ChainMove
        {
            /// <summary>
            /// Creates a shallow copy by copying the contained moves into a new list
            /// </summary>
            private InterPickupAndPlaceMove(InterPickupAndPlaceMove source)
                : base(source.Moves)
            {
            }

            /// <summary>
            /// Start building the PickupAndPlaceMove by starting with just the PickUpMove
            /// </summary>
            public InterPickupAndPlaceMove(PickUpMove pickupMove)
            {
                base.AddToChain(pickupMove);
            }

            /// <summary>
            /// Gets the PickUpMove part of this chain of moves
            /// </summary>
            public PickUpMove PickUpMove { get { return (PickUpMove)base[0]; } }

            /// <summary>
            /// Create a shallow copy of this move
            /// </summary>
            /// <returns></returns>
            public InterPickupAndPlaceMove ShallowCopy()
            {
                return new InterPickupAndPlaceMove(this);
            }

            string _notated;
            public override string Notate()
            {
                if (_notated == null)
                {
                    var sb = new System.Text.StringBuilder();
                    var pickup = this.PickUpMove;
                    if (pickup.PickUpCount > 1 || pickup.Remaining != 0)
                        sb.Append(pickup.PickUpCount);
                    sb.Append(pickup.Position.Describe());
                    var delta = ((InterPlacePieceMove)Moves[1]).Pos - pickup.Position;
                    sb.Append(Direction.DescribeDelta(delta));
                    if (Moves.Count > 2)
                    {
                        for (int i = 1; i < Moves.Count; )
                        {
                            int unstackCount = 1;
                            while (i + unstackCount < Moves.Count &&
                                ((InterPlacePieceMove)Moves[i]).Pos == ((InterPlacePieceMove)Moves[i + unstackCount]).Pos)
                                unstackCount++;
                            if (i > 1 || i + unstackCount < Moves.Count)
                                sb.Append(unstackCount);
                            i += unstackCount;
                        }
                    }
                    _notated = sb.ToString();
                }
                return _notated;
            }

            public IEnumerable<int> GetUnstackCounts()
            {
                for (int i = 1; i < Moves.Count; )
                {
                    int unstackCount = 1;
                    while (i + unstackCount < Moves.Count &&
                        ((InterPlacePieceMove)Moves[i]).Pos == ((InterPlacePieceMove)Moves[i + unstackCount]).Pos)
                        unstackCount++;
                    i += unstackCount;
                    yield return unstackCount;
                }
            }

            public override string ToString()
            {
                return Notate();
            }

            public int? UnstackDirection
            {
                get
                {
                    if (Moves.Count < 2)
                        return null;
                    else
                    {
                        var delta = ((InterPlacePieceMove)Moves[1]).Pos - PickUpMove.Position;
                        return Direction.FromDelta(delta);
                    }
                }
            }
        }
    }
}
