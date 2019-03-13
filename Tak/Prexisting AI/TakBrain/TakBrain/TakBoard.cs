using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// Represents the current state of the Tak board
    /// </summary>
    public class TakBoard : IEquatable<TakBoard>
    {
        #region Constructors
        public TakBoard(int n)
        {
            Size = n;
            Board = new PieceStack[n,n];

            for(int i=0; i<n; i++)
            {
                for(int j=0; j<n; j++)
                {
                    Board[i, j] = new PieceStack();
                }
            }
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="src"></param>
        public TakBoard(TakBoard src)
        {
            Size = src.Size;
            Board = new PieceStack[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Board[i, j] = new PieceStack(src.Board[i,j]);
                }
            }
        }
        #endregion
        #region Stacks
        /// <summary>
        /// The size of the board edges.  Also indicates the carry limit of the board
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The 2d array of stacks that form the board
        /// </summary>
        private PieceStack[,] Board { get; set; }

        public PieceStack this[int i, int j]
        {
            get
            {
                return Board[i, j];
            }
        }
        #endregion
        #region ToString
        public override string ToString()
        {
            string s = "   ";

            for (int i = 0; i < Size; i++)
            {
                s += string.Format("{0} ", (char)('a' + i));
            }
            s += "\n   ";
            for (int i = 0; i < Size; i++)
            {
                s += "--";
            }
            s += "\n";

            for (int i=Size-1; i>=0; i--)
            {
                s += string.Format("{0}| ", i + 1);
                for(int j=0; j<Size; j++)
                {
                    s += string.Format("{0} ",Board[i, j].Size);
                }
                s += string.Format("|{0}\n",i+1);
            }

            s += "   ";
            for (int i = 0; i < Size; i++)
            {
                s += "--";
            }
            s += "\n   ";
            for (int i = 0; i < Size; i++)
            {
                s += string.Format("{0} ", (char)('a' + i));
            }
            s += "\n";
            
            return s;
        }
        #endregion
        #region Victory & Endgame
        /// <summary>
        /// Is there a road on the board (i.e. an orthogonal path of flat pieces and captsones that
        /// connects two opposite board edges)
        /// </summary>
        public bool HasRoad()
        {
            return HasRoad(TakPiece.PieceColor.Black) || HasRoad(TakPiece.PieceColor.White);
        }

        public bool HasRoad(TakPiece.PieceColor player)
        {
            BoardGraph g = new BoardGraph(this, player);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (g.IsConnected(0, i, Size - 1, j) || // vertical road
                        g.IsConnected(i, 0, j, Size - 1))  // horizontal road
                        return true;

                }
            }

            // we've checked every possibility, so return false
            return false;
        }

        /// <summary>
        /// Is the entire board full?
        /// </summary>
        public bool IsFull()
        {
            foreach (PieceStack s in Board)
                if (s.Size == 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Get the flat score of the board
        /// 
        /// The score is |White|-|Black|, so a positive value means white is winning
        /// 
        /// Note that only flat pieces are counted; walls and capstones are not
        /// </summary>
        public int FlatScore()
        {
            int nWhite = 0;
            int nBlack = 0;
            foreach (PieceStack s in Board)
            {
                if (s.Owner == TakPiece.PieceColor.White && s.Top.Type == TakPiece.PieceType.Flat)
                    nWhite++;
                else if (s.Owner == TakPiece.PieceColor.Black && s.Top.Type == TakPiece.PieceType.Flat)
                    nBlack++;
            }
            return nWhite - nBlack;
        }
        #endregion
        #region Drawing
        /// <summary>
        /// Draw a really crude board that shows the stacks and their contents
        /// </summary>
        /// <returns></returns>
        public Bitmap Draw()
        {
            const int SQUARE_SIZE = 70;
            const int FLAT_SIZE = 40;
            const int MARGIN = 40;

            // the XY position of the centre of the bottom surface of each type of piece, given our (admittedly crappy) graphics
            // measured from the upper-left corner
            Point flatOffset = new Point(46, 46);
            Point wallOffset = new Point(16, 68);
            Point capstoneOffset = new Point(18, 76);

            if(DrawingCanvas == null)
                DrawingCanvas = new Bitmap(SQUARE_SIZE * Size + 2*MARGIN, SQUARE_SIZE * Size + 2 * MARGIN);

            Brush light = new SolidBrush(Color.FromArgb(0xff, 0xd4, 0xbe, 0x8c));
            Brush dark = new SolidBrush(Color.FromArgb(0xff, 0x7f, 0x71, 0x54));
            using (Graphics g = Graphics.FromImage(DrawingCanvas))
            {
                g.Clear(Color.Transparent);

                // draw the base gridS
                for(int i=0; i<Size; i++)
                {
                    for(int j=0; j<Size; j++)
                    {
                        Point corner = new Point(i * SQUARE_SIZE + MARGIN, j * SQUARE_SIZE + MARGIN);
                        if (i % 2 == j % 2)
                            g.FillRectangle(Brushes.DarkGray, corner.X, corner.Y, SQUARE_SIZE, SQUARE_SIZE);
                        else
                            g.FillRectangle(Brushes.LightGray, corner.X, corner.Y, SQUARE_SIZE, SQUARE_SIZE);
                    }

                    g.DrawString(string.Format("{0}",(char)('A' + i)), 
                        new Font(FontFamily.GenericMonospace, SQUARE_SIZE / 4), 
                        Brushes.Black, 
                        new Point(MARGIN + SQUARE_SIZE * i + SQUARE_SIZE / 2, DrawingCanvas.Height - MARGIN));
                    g.DrawString(string.Format("{0}",i+1), 
                        new Font(FontFamily.GenericMonospace, SQUARE_SIZE / 4), 
                        Brushes.Black, 
                        new Point(MARGIN / 2, DrawingCanvas.Height-MARGIN - SQUARE_SIZE*(i+1) + SQUARE_SIZE/2));
                }

                // draw the stacks starting from the back
                for(int i=Size-1; i>=0; i--)
                {
                    for(int j=0; j<Size; j++)
                    {
                        List<TakPiece.PieceColor> colors = Board[i, j].Colors;

                        Point squareTL = new Point(j * SQUARE_SIZE + MARGIN, (Size - 1 - i) * SQUARE_SIZE + MARGIN);
                        Point squareCtr = new Point(squareTL.X + SQUARE_SIZE / 2, squareTL.Y + SQUARE_SIZE / 2);
                        Point pieceTL = new Point(squareCtr.X - 2*FLAT_SIZE / 3, squareCtr.Y - FLAT_SIZE / 3);
                        for (int k=0; k<colors.Count; k++)
                        {
                            Brush outline, fill;
                            if(colors[k] == TakPiece.PieceColor.Black)
                            {
                                outline = light;
                                fill = dark;
                            }
                            else
                            {
                                outline = dark;
                                fill = light;
                            }

                            if(k<colors.Count-1)
                            {
                                g.FillRectangle(fill, pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);
                                g.DrawRectangle(new Pen(outline, 2), pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);

                                pieceTL.Y -= 8;
                                pieceTL.X += 8;
                            }
                            else
                            {
                                if(Board[i,j].Top.IsCapstone)
                                {
                                    g.FillEllipse(fill, pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);
                                    g.DrawEllipse(new Pen(outline, 2), pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);
                                }
                                else if (Board[i, j].Top.IsWall)
                                {
                                    g.DrawLine(new Pen(outline, 12), pieceTL, new Point(pieceTL.X + FLAT_SIZE, pieceTL.Y + FLAT_SIZE));
                                    g.DrawLine(new Pen(fill, 8), new Point(pieceTL.X+2, pieceTL.Y+2), new Point(pieceTL.X + FLAT_SIZE-2, pieceTL.Y + FLAT_SIZE-2));
                                }
                                else
                                {
                                    g.FillRectangle(fill, pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);
                                    g.DrawRectangle(new Pen(outline, 2), pieceTL.X, pieceTL.Y, FLAT_SIZE, FLAT_SIZE);
                                }
                            }
                        }
                    }
                }
            }

            return DrawingCanvas;
        }

        private static Bitmap DrawingCanvas { get; set; }
        #endregion
        #region Symmetry checking

        /// <summary>
        /// Compare two boards and return true if they are either identical or rotations/reflections of each other
        /// 
        /// TODO: right now this doesn't implement reflections and rotations
        /// 
        /// TODO: figure out an efficient way of checking for isomorphisms
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(TakBoard b)
        {
            bool match;
            int aStacks, aPieces;
            int bStacks, bPieces;

            long[,] aarr = ToIntArray(out aStacks, out aPieces);
            long[,] barr = b.ToIntArray(out bStacks, out bPieces);

            if (aStacks == bStacks && aPieces == bPieces)
            {

                // check all board rotations
                match = IdenticalBoards(aarr, barr);
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }

                // flip the board across the vertical axis and try again
                if (!match)
                {
                    Rotate90(barr);
                    FlipVertical(barr);
                    match = IdenticalBoards(aarr, barr);
                }
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }
                if (!match)
                {
                    Rotate90(barr);
                    match = IdenticalBoards(aarr, barr);
                }
            }
            else
                match = false;

            return match;
        }

        private long[,] ToIntArray(out int nStacks, out int nPieces)
        {
            long[,] arr = new long[Size, Size];
            const int CAP_FLAG = (1 << 63);
            const int WALL_FLAG = (1 << 62);
            const int WHITE_FLAG = (1 << 60);

            nStacks = 0;
            nPieces = 0;

            for(int i=0; i<Size; i++)
            {
                for(int j=0; j<Size; j++)
                {
                    nPieces += Board[i, j].Size;
                    if(Board[i,j].Size != 0)
                    {
                        nStacks++;
                        arr[i, j] = Board[i, j].Size;
                        if (Board[i, j].Type == TakPiece.PieceType.Wall)
                            arr[i, j] |= WALL_FLAG;
                        if (Board[i, j].Type == TakPiece.PieceType.Capstone)
                            arr[i, j] |= CAP_FLAG;
                        if (Board[i, j].Owner == TakPiece.PieceColor.White)
                            arr[i, j] |= WHITE_FLAG;
                    }
                }
            }

            return arr;
        }

        private void Rotate90(long[,] arr)
        {
            long tmp;

            for(int i=0; i<Size; i++)
            {
                for(int j=0; j<Size; j++)
                {
                    tmp = arr[i, j];
                    arr[i, j] = arr[j, Size - 1 - i];
                    arr[j, Size - 1 - i] = tmp;
                }
            }
        }

        private void FlipVertical(long[,] arr)
        {
            long tmp;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    tmp = arr[i, Size-1-j];
                    arr[i, j] = arr[i, Size - 1 - j];
                    arr[i, Size - 1 - j] = tmp;
                }
            }
        }

        /// <summary>
        /// Do a stack-by-stack comparison of two boards and return true if they match exactly
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool IdenticalBoards(long[,] a, long[,] b)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                    if (a[i, j] != b[i, j])
                        return false;
            }
            return true;
        }
        #endregion
    }
}
