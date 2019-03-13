using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakEngine
{
    public static class Helper
    {
        /// <summary>
        /// Enumerate all legal moves in the current board position
        /// </summary>
        /// <param name="dest">Destination list into which moves will be added</param>
        /// <param name="game">Current game state</param>
        /// <param name="moveOrder">Order in which board positions will be considered</param>
        public static void EnumerateMoves(IList<IMove> dest, GameState game, IList<BoardPosition> moveOrder)
        {
            int player = game.Ply & 1;
            if (game.Ply < 2)
            {
                // place enemy flat stone on empty squares
                player = player ^ 1;
                foreach (var pos in moveOrder)
                {
                    if (!game[pos].HasValue)
                        dest.Add(new PlacePieceMove(Piece.MakePieceID(Piece.Stone_Flat, player), pos));
                }
            }
            else
            {
                // place stones on empty squares
                var sremain = game.StonesRemaining[player];
                var cremain = game.CapRemaining[player];
                foreach (var pos in moveOrder)
                {
                    if (!game[pos].HasValue)
                    {
                        if (sremain > 0)
                        {
                            dest.Add(new PlacePieceMove(Piece.MakePieceID(Piece.Stone_Flat, player), pos));
                            dest.Add(new PlacePieceMove(Piece.MakePieceID(Piece.Stone_Standing, player), pos));
                        }
                        if (cremain > 0)
                            dest.Add(new PlacePieceMove(Piece.MakePieceID(Piece.Stone_Cap, player), pos));
                    }
                }

                // Move stacks

                var placeCounts = new List<int>();
                foreach (var pos in moveOrder)
                {
                    var stack = game.Board[pos.X, pos.Y];
                    if (stack.Count == 0)
                        continue;
                    var topPiece = stack[stack.Count - 1];
                    var topStone = Piece.GetStone(topPiece);
                    if (Piece.GetPlayerID(topPiece) != player)
                        continue;

                    for (int pickupCount = 1; pickupCount <= Math.Min(stack.Count, game.Size); pickupCount++)
                    {
                        for (int dir = 0; dir < 4; dir++)
                        {
                            placeCounts.Clear();
                            EnumUnstack(dest, game, pickupCount, pickupCount, placeCounts, pos, dir, topStone == Piece.Stone_Cap);
                        }
                    }
                }
            }
        }

        static bool EnumUnstack(IList<IMove> dest, GameState game, int pickupCount, int remaining, List<int> placeCounts, BoardPosition pickupPos, int dir, bool capped)
        {
            var dx = Direction.DirX[dir];
            var dy = Direction.DirY[dir];
            var target = new BoardPosition(
                pickupPos.X + dx * (placeCounts.Count + 1),
                pickupPos.Y + dy * (placeCounts.Count + 1));
            if (target.X >= game.Size || target.Y >= game.Size || target.X < 0 || target.Y < 0)
                return false;
            var desttoppiece = game[target];
            if (desttoppiece.HasValue && Piece.GetStone(desttoppiece.Value) == Piece.Stone_Standing)
            {
                if (remaining != 1 || !capped)
                    return false;
            }
            if (desttoppiece.HasValue && Piece.GetStone(desttoppiece.Value) == Piece.Stone_Cap)
                return false;

            placeCounts.Add(0);
            for (int mycount = remaining; mycount >= 1; mycount--)
            {
                placeCounts[placeCounts.Count - 1] = mycount;
                if (mycount == remaining)
                {
                    var arrPlaceCounts = new int[placeCounts.Count];
                    for (int i = 0; i < arrPlaceCounts.Length; i++)
                        arrPlaceCounts[i] = placeCounts[i];
                    dest.Add(new PickupAndPlaceMove(pickupPos, pickupCount, game.Board[pickupPos.X, pickupPos.Y].Count, dir, arrPlaceCounts));
                }
                else
                {
                    if (!EnumUnstack(dest, game, pickupCount, remaining - mycount, placeCounts, pickupPos, dir, capped))
                        break;
                }
            }
            placeCounts.RemoveAt(placeCounts.Count - 1);
            return true;
        }
    }
}
