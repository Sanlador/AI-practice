using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// A single piece in a Tak board
    /// 
    /// This class represents a flat or a wall
    /// </summary>
    public class TakPiece
    {
        public TakPiece(PieceColor colour)
        {
            Color = colour;
        }

        public TakPiece(TakPiece src)
        {
            Color = src.Color;
            IsWall = src.IsWall;
        }

        /// <summary>
        /// Is this piece a wall?
        /// </summary>
        public virtual bool IsWall { get; set; }

        /// <summary>
        /// Normal pieces can never be capstones
        /// </summary>
        public virtual bool IsCapstone { get { return false; } }

        public PieceColor Color { get; private set; }

        public virtual PieceType Type
        {
            get
            {
                if (IsWall)
                    return PieceType.Wall;
                else
                    return PieceType.Flat;
            }
        }

        /// <summary>
        /// The colours of the Tak pieces
        /// </summary>
        public enum PieceColor
        {
            Black,
            White
        }

        /// <summary>
        /// The types of pieces we can drop onto the board
        /// </summary>
        public enum PieceType
        {
            /// <summary>
            /// A standard flat stone
            /// </summary>
            Flat,

            /// <summary>
            /// A vertical wall stone
            /// </summary>
            Wall,

            /// <summary>
            /// The capstone
            /// </summary>
            Capstone
        }
    }
}
