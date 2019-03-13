using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    public class PieceStack
    {
        /// <summary>
        /// Create an empty stack of pieces
        /// </summary>
        public PieceStack()
        {
            Pieces = new List<TakPiece>();
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="src"></param>
        public PieceStack(PieceStack src) : this()
        {
            foreach (TakPiece piece in src.Pieces)
            {
                if (piece.IsCapstone)
                    Pieces.Add(new Capstone(piece as Capstone));
                else
                    Pieces.Add(new TakPiece(piece));
            }
        }

        /// <summary>
        /// The pieces in the stack
        /// 
        /// The 0 index is the bottom of the stack, the Count-1 index is the top of the stack
        /// </summary>
        private List<TakPiece> Pieces;

        /// <summary>
        /// The number of pieces in the stack
        /// </summary>
        public int Size
        {
            get { return Pieces.Count; }
        }

        public void Place(TakPiece piece)
        {
            if (Size != 0)
                throw new Exception("Cannot place a piece on a non-empty stack");
            else
                Pieces.Add(piece);
        }

        /// <summary>
        /// Drop n pieces off the bottom of the stack onto the target stack
        /// 
        /// This is used when moving the stack
        /// </summary>
        /// <param name="target"></param>
        /// <param name="n"></param>
        public void Drop(PieceStack target, int n = 1)
        {
            if (n > Size)
                throw new ArgumentOutOfRangeException(string.Format("Cannot drop {0} pieces from a stack of size {1}", n, Size));

            // flatten the top piece if it was a wall
            // NOTE: we're assuming the move is legal, so there is no check
            if (target.Pieces.Count > 0)
                target.Top.IsWall = false;

            for (int i = 0; i < n; i++)
            {
                target.Pieces.Add(Pieces[0]);
                Pieces.RemoveAt(0);
            }
        }

        /// <summary>
        /// Grab the top n pieces from this stack and return them as a new stack
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public PieceStack Grab(int n)
        {
            if (n > Size)
                throw new ArgumentOutOfRangeException(string.Format("Cannot grab {0} pieces from a stack of size {1}", n, Size));

            PieceStack s = new PieceStack();
            for (int i = 0; i < n; i++)
            {
                s.Pieces.Insert(0, Pieces.Last());
                Pieces.RemoveAt(Size - 1);
            }
            return s;
        }

        /// <summary>
        /// Grab the top n pieces from the stack, but don't actually modify the stack; just make a copy
        /// 
        /// This is used when checking for legal moves, since we don't want to disrupt the board state
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal PieceStack NonDestructiveGrab(int n)
        {
            if (n > Size)
                throw new ArgumentOutOfRangeException(string.Format("Cannot grab {0} pieces from a stack of size {1}", n, Size));

            PieceStack s = new PieceStack();
            for (int i = 0; i < n; i++)
            {
                s.Pieces.Insert(0, Pieces[Size - 1 - i]);
            }
            return s;
        }

        /// <summary>
        /// Remove n pieces from the bottom of the stack and make them evaporate
        /// 
        /// Used in conjuction with <see cref="NonDestructiveGrab(int)"/> to check legal moves
        /// </summary>
        /// <param name="n"></param>
        internal void Drop(int n)
        {
            if (n > Size)
                throw new ArgumentOutOfRangeException(string.Format("Cannot drop {0} pieces from a stack of size {1}", n, Size));

            for (int i = 0; i < n; i++)
            {
                Pieces.RemoveAt(0);
            }
        }

        /// <summary>
        /// Verify if we can drop n pieces on this stack onto the target space
        /// </summary>
        /// <param name="target"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool CanDrop(PieceStack target, int n)
        {
            // if the target space is empty or contains flats we're fine
            if (target.Size == 0 || (!target.Top.IsWall && !target.Top.IsCapstone))
                return true;

            // otherwise check if we're placing a capstone on any non-capstone we're fine
            else if (Pieces[0].IsCapstone && !target.Top.IsCapstone)
                return true;

            // otherwise this is an illegal move
            return false;
        }

        /// <summary>
        /// The piece on top of the stack
        /// </summary>
        public TakPiece Top { get { return Pieces.Last(); } }

        /// <summary>
        /// Get the player who owns this stack
        /// 
        /// Returns null if the stack is empty
        /// </summary>
        public TakPiece.PieceColor? Owner
        {
            get
            {
                if (Size == 0)
                    return null;
                else
                    return Top.Color;
            }
        }

        public TakPiece.PieceType? Type
        {
            get
            {
                if (Size == 0)
                    return null;
                else
                {
                    if (Top.IsCapstone)
                        return TakPiece.PieceType.Capstone;
                    else if (Top.IsWall)
                        return TakPiece.PieceType.Wall;
                    else
                        return TakPiece.PieceType.Flat;
                }
            }
        }

        /// <summary>
        /// Get the list of all the colours of stones in the stack, starting at the bottom and working up
        /// </summary>
        public List<TakPiece.PieceColor> Colors
        {
            get
            {
                List<TakPiece.PieceColor> colors = new List<TakPiece.PieceColor>();
                foreach (TakPiece p in Pieces)
                    colors.Add(p.Color);
                return colors;
            }
        }

        public static bool operator ==(PieceStack a, PieceStack b)
        {
            if(a.Size == b.Size)
            {
                for(int i=0; i<a.Size; i++)
                {
                    if (a.Pieces[i].Color != b.Pieces[i].Color)
                        return false;
                }

                if (a.Size > 0)
                    return a.Top.Type == b.Top.Type;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(PieceStack a, PieceStack b)
        {
            return !(a == b);
        }
    }
}
