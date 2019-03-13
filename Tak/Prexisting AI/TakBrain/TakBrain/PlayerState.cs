using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// The state of the player's  pieces
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// Create a new player with the number of pieces necessary for the size of the board
        /// </summary>
        /// <param name="color"></param>
        /// <param name="boardSize"></param>
        public PlayerState(TakPiece.PieceColor color, int boardSize)
        {
            switch(boardSize)
            {
                case 3:
                    NumPieces = 10;
                    NumCapstones = 0;
                    break;

                case 4:
                    NumPieces = 15;
                    NumCapstones = 0;
                    break;

                case 5:
                    NumPieces = 21;
                    NumCapstones = 1;
                    break;

                case 6:
                    NumPieces = 30;
                    NumCapstones = 1;
                    break;

                case 7:
                    NumPieces = 40;
                    NumCapstones = 2;
                    break;

                case 8:
                    NumPieces = 50;
                    NumCapstones = 2;
                    break;
            }
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="src"></param>
        public PlayerState(PlayerState src)
        {
            NumCapstones = src.NumCapstones;
            NumPieces = src.NumPieces;
        }

        /// <summary>
        /// The number of pieces available in the player's pool
        /// </summary>
        public int NumPieces { get; set; }

        /// <summary>
        /// The number of capstones available in the player's pool
        /// </summary>
        public int NumCapstones { get; set; }
    }
}
