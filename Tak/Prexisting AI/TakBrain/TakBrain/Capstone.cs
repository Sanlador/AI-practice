using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    public class Capstone : TakPiece
    {
        public Capstone(PieceColor colour) : base(colour)
        {

        }

        public Capstone(Capstone src) : base(src)
        {

        }

        /// <summary>
        /// The capstone is, logically, always a capstone
        /// </summary>
        public override bool IsCapstone { get { return true; } }

        public override PieceType Type
        {
            get
            {
                return PieceType.Capstone;
            }
        }

        /// <summary>
        /// Capstones can never be walls
        /// </summary>
        public override bool IsWall { get { return false; } set { } }
    }
}
