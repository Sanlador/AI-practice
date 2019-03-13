using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// Encapsulates the entire state of the game at any given time
    /// 
    /// Includes counters for the players' pieces and the board state
    /// </summary>
    public class GameState
    {
        public GameState(TakBoard board)
        {
            Board = board;
            White = new PlayerState(TakPiece.PieceColor.White, Board.Size);
            Black = new PlayerState(TakPiece.PieceColor.Black, Board.Size);
            TurnNumber = 0;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="src"></param>
        public GameState(GameState src)
        {
            Board = new TakBoard(src.Board);
            White = new PlayerState(src.White);
            Black = new PlayerState(src.Black);
            TurnNumber = src.TurnNumber;
        }

        /// <summary>
        /// The board the players are playing on
        /// </summary>
        public TakBoard Board { get; set; }

        /// <summary>
        /// The state of the black player's pieces
        /// </summary>
        public PlayerState Black { get; set; }

        /// <summary>
        /// The state of the white player's pieces
        /// </summary>
        public PlayerState White { get; set; }

        /// <summary>
        /// The current turn number
        /// 
        /// This should be incremented after BOTH players have moved
        /// 
        /// Note that on turn 0 each player places one of their opponents' flat stones
        /// </summary>
        public int TurnNumber { get; set; }

        /// <summary>
        /// Direct access to each player's pieces by color
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public PlayerState this[TakPiece.PieceColor player]
        {
            get
            {
                if (player == TakPiece.PieceColor.White)
                    return White;
                else
                    return Black;
            }
        }

        /// <summary>
        /// Check if the game is over
        /// 
        /// The game ends if there is a road win, or if the whole board is full, or if either player is out of pieces
        /// </summary>
        public bool GameOver
        {
            get
            {
                return Board.HasRoad() || Board.IsFull() || Black.NumPieces == 0 || White.NumPieces == 0;
            }
        }

        /// <summary>
        /// Get the winner, or null if it is a draw
        /// </summary>
        public TakPiece.PieceColor? Winner(TakPiece.PieceColor currentPlayer)
        {
            TakPiece.PieceColor otherPlayer = currentPlayer == TakPiece.PieceColor.White ? TakPiece.PieceColor.Black : TakPiece.PieceColor.White;

            // check road victories first (current player always wins if they made a road)
            if (Board.HasRoad(currentPlayer))
                return currentPlayer;
            else if (Board.HasRoad(otherPlayer))
                return otherPlayer;

            // if no roads, check the flat score
            int score = Board.FlatScore();
            if (score > 0)
                return TakPiece.PieceColor.White;
            else if (score < 0)
                return TakPiece.PieceColor.Black;
            else
                return null;
        }
        #region Heuristics
        /// <summary>
        /// Use some heuristics to evaluate how good a board position this is for the current player
        /// 
        /// Positive scores indicate good outcomes, negative scores are bad
        /// </summary>
        /// <returns></returns>
        public double Evaluate(TakPiece.PieceColor currentPlayer)
        {
            double score = 0;

            TakPiece.PieceColor otherPlayer = currentPlayer == TakPiece.PieceColor.Black ? TakPiece.PieceColor.White : TakPiece.PieceColor.Black;

            // check for all victory conditions and return extreme values for any of them
            // (note that flats can result in a perfect draw, so we return 0 in that case
            if (Board.HasRoad(currentPlayer))
                score = int.MaxValue;
            else if (Board.HasRoad(otherPlayer))
                score = int.MinValue;
            else if(Board.IsFull() || Black.NumPieces == 0 || White.NumPieces == 0)
            {
                int flats = Board.FlatScore();
                if ((currentPlayer == TakPiece.PieceColor.White && flats > 0) ||
                    (currentPlayer == TakPiece.PieceColor.Black && flats < 0))
                    score = int.MaxValue;
                else if (flats != 0)
                    score = int.MinValue;
                else
                    score = 0;
            }
            else
            {
                // otherwise use a variety of metrics to evaluate the board state
                // TODO decide what these metrics are
                // some ideas:
                // - flat coverage
                // - length of longest path through board graph
                // - number of pieces along same row/column
                // - number of captives/reserves
                // - number of spaces threatened
                // - number of pieces that threaten each space
                // - number of 2x2 squares of our own pieces
                // - number of opposing pieces threatened
                // - number of own pieces threatened

                int flatScore = Board.FlatScore() * (otherPlayer == TakPiece.PieceColor.White ? -10 : 10);
                int numSquares = CountSquares(currentPlayer);
                int maxLine = CountMostOrthogonalInRowOrColumn(currentPlayer);
                int mostFullRowCol = CountMostDistinctRowsOrColumns(currentPlayer);

                score = flatScore + numSquares + maxLine + mostFullRowCol;

                //Console.WriteLine("{0} = {1} {2} {3} {4}", score, deltaCoverage, deltaSquares, deltaOrtho, deltaDistinct);
            }

            return score;
        }

        /// <summary>
        /// Count the number of pieces the player has in the row/column that contains the most pieces
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int CountMostOrthogonalInRowOrColumn(TakPiece.PieceColor player)
        {
            int most = 0;
            int count;
            for(int i=0; i<Board.Size; i++)
            {
                count = 0;
                for (int j = 0; j < Board.Size; j++)
                {
                    if (Board[i, j].Owner == player && Board[i, j].Type != TakPiece.PieceType.Wall)
                        count++;
                }
                if (count > most)
                    most = count;

                count = 0;
                for (int j = 0; j < Board.Size; j++)
                {
                    if (Board[j, i].Owner == player && Board[j, i].Type != TakPiece.PieceType.Wall)
                        count++;
                }
                if (count > most)
                    most = count;
            }

            return most;
        }

        /// <summary>
        /// Count the number of distinct rows or columns we have pieces in
        /// 
        /// i.e. if we have pieces in rows 0, 1, 4 and columns 2,3,4, and 5 we'll return 4
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int CountMostDistinctRowsOrColumns(TakPiece.PieceColor player)
        {
            int most = 0;
            int count;
            for (int i = 0; i < Board.Size; i++)
            {
                count = 0;
                for (int j = 0; j < Board.Size; j++)
                {
                    if (Board[i, j].Owner == player && Board[i, j].Type != TakPiece.PieceType.Wall)
                    {
                        count++;
                        break;
                    }
                }
                if (count > most)
                    most = count;

                count = 0;
                for (int j = 0; j < Board.Size; j++)
                {
                    if (Board[j, i].Owner == player && Board[j, i].Type != TakPiece.PieceType.Wall)
                    {
                        count++;
                        break;
                    }
                }
                if (count > most)
                    most = count;
            }

            return most;
        }

        /// <summary>
        /// Count the number of 2x2 squares of non-wall pieces we own
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int CountSquares(TakPiece.PieceColor player)
        {
            int count = 0;
            for (int i = 0; i < Board.Size-1; i++)
            {
                for (int j = i; j < Board.Size-1; j++)
                {
                    if ((Board[i, j].Owner == player && Board[i, j].Type != TakPiece.PieceType.Wall) &&
                        (Board[i+1, j].Owner == player && Board[i+1, j].Type != TakPiece.PieceType.Wall) &&
                        (Board[i, j+1].Owner == player && Board[i, j+1].Type != TakPiece.PieceType.Wall) &&
                        (Board[i + 1, j + 1].Owner == player && Board[i + 1, j+1].Type != TakPiece.PieceType.Wall))
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
        #endregion
    }
}
