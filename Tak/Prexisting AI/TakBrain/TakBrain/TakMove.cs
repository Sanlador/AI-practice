using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// A single move in the game
    /// </summary>
    public class TakMove
    {
        #region Constructors
        private TakMove(TakPiece.PieceColor player, int row, int column, MoveType type)
        {
            PieceColor = player;
            Row = row;
            Column = column;
            Type = type;
        }

        /// <summary>
        /// Create a placement move where we place a new piece on the location
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="piece"></param>
        public TakMove(TakPiece.PieceColor player, int row, int column, TakPiece.PieceType piece= TakPiece.PieceType.Flat) : this(player,row,column,MoveType.Place)
        {
            Piece = piece;
        }

        /// <summary>
        /// Create a move where we pick up a stack and drop pieces as we move in a direction
        /// 
        /// The sum of all values in drops must equal the number of pieces picked up. So if
        /// we pick up a single piece and move it drops should be [1].
        /// 
        /// If we pick up 6 pieces, and drop 1, 1, 2, 1 it should be [1, 1, 2, 1]
        /// </summary>
        /// <param name="player"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        /// <param name="drops"></param>
        public TakMove(TakPiece.PieceColor player, int row, int column, MoveDirection direction, List<int> drops) : this(player,row,column,MoveType.Move)
        {
            Drops = drops;
            Direction = direction;
        }

        /// <summary>
        /// Parse a string representing a move
        /// 
        /// See README.md for details on notation
        /// 
        /// The notation should not contain any spaces
        /// </summary>
        /// <param name="player"></param>
        /// <param name="notation"></param>
        public TakMove(TakPiece.PieceColor player, string notation)
        {
            notation = notation.Trim();

            try
            {
                PieceColor = player;

                if (notation.Contains(">") || notation.Contains("<") || notation.Contains("-") || notation.Contains("+"))
                {
                    // if we contain any of the move symbols, assume we're a move

                    Type = MoveType.Move;

                    int coordStart = FirstLetterAt(notation);
                    string coord = notation.Substring(coordStart, 2);
                    int row, col;
                    ParseCoord(coord, out row, out col);
                    Row = row;
                    Column = col;

                    char dir = notation[coordStart + 2];
                    switch (dir)
                    {
                        case '+':
                            Direction = MoveDirection.Up;
                            break;
                        case '-':
                            Direction = MoveDirection.Down;
                            break;
                        case '<':
                            Direction = MoveDirection.Left;
                            break;
                        case '>':
                            Direction = MoveDirection.Right;
                            break;
                    }

                    Drops = new List<int>();
                    string dropstring = notation.Substring(coordStart + 3);
                    foreach (char ch in dropstring)
                    {
                        Drops.Add(ch - '0');
                    }
                }
                else
                {
                    if (notation.Length == 2)
                    {
                        Piece = TakPiece.PieceType.Flat;

                        int row, col;
                        ParseCoord(notation, out row, out col);
                        Row = row;
                        Column = col;
                    }
                    else
                    {
                        switch (notation.ToLower()[0])
                        {
                            case 's':
                                Piece = TakPiece.PieceType.Wall;
                                break;

                            case 'c':
                                Piece = TakPiece.PieceType.Capstone;
                                break;
                        }

                        int row, col;
                        ParseCoord(notation.Substring(1, 2), out row, out col);
                        Row = row;
                        Column = col;
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception("Failed to parse move", e);
            }
        }

        private static int FirstLetterAt(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (char.IsLetter(s[i]))
                    return i;
            return -1;
        }

        private static void ParseCoord(string coord, out int row, out int col)
        {
            coord = coord.ToLower();
            char file = coord[0];
            char rank = coord[1];
            col = file - 'a';
            row = rank - '1';
        }
        #endregion
        #region Properties
        /// <summary>
        /// The board row that the move originates on
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// The board column that the move originates on
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// The type of move this is
        /// </summary>
        public MoveType Type { get; private set; }

        /// <summary>
        /// The direction of a stack-spilling move
        /// 
        /// Only set if the move is of type <see cref="MoveType.Move"/> 
        /// </summary>
        public MoveDirection Direction { get; private set; }

        /// <summary>
        /// The number of pieces dropped at each stage of the move
        /// 
        /// Only set if the move is of type <see cref="MoveType.Move"/> 
        /// </summary>
        public List<int> Drops { get; private set; }

        /// <summary>
        /// Get the total number of pieces moved
        /// </summary>
        public int NumMoved
        {
            get
            {
                if (Drops == null || Drops.Count == 0)
                    return 0;

                int n = 0;
                foreach (int i in Drops)
                    n += i;
                return n;
            }
        }

        /// <summary>
        /// The color of the piece being placed
        /// </summary>
        public TakPiece.PieceColor PieceColor { get; private set; }
        #endregion
        #region enums
        /// <summary>
        /// The type of piece we're placing
        /// 
        /// Only set of the move is of type <see cref="MoveType.Place"/> 
        /// </summary>
        public TakPiece.PieceType Piece { get; private set; }
        
        /// <summary>
        /// The direction of a move
        /// </summary>
        public enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        /// <summary>
        /// The types of move we can make
        /// </summary>
        public enum MoveType
        {
            /// <summary>
            /// Placing a piece at an empty space on the board
            /// </summary>
            Place,

            /// <summary>
            /// Moving a stack in a direction
            /// </summary>
            Move
        }
        #endregion
        #region ToString
        /// <summary>
        /// Get the move as a string
        /// 
        /// The move notation is as follows:
        /// 
        /// Placement: e.g. a2, Cg5, Sb3
        ///     [S|C]{X}{Y} where...
        ///         - S indicates the piece is standing
        ///         - C indicates a capstone
        ///         - S or C may be omitted if the stone is flat
        ///         - X is the column as a letter [lower-case] (a, b, c, ...)
        ///         - Y is is the row as a number (1, 2, 3, ...)
        ///         
        /// Move: e.g. a2+121. b8&lt;31
        ///     {X}{Y}{D}{...} where...
        ///         - X and Y are as described above
        ///         - D is one of +, -, &lt;, &gt; indicating the direction of the move
        ///         - ... is a list of numbers representing the number of pieces dropped at each step of the move
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (Type)
            {
                case MoveType.Place:
                    string prefix;
                    switch(Piece)
                    {
                        case TakPiece.PieceType.Capstone:
                            prefix = "C";
                            break;

                        case TakPiece.PieceType.Wall:
                            prefix = "S";
                            break;

                        default:
                            prefix = "";
                            break;
                    }
                    return string.Format("{0}{1}", prefix, Coord);

                case MoveType.Move:
                    string dir;
                    switch(Direction)
                    {
                        case MoveDirection.Up:
                            dir = "+";
                            break;

                        case MoveDirection.Down:
                            dir = "-";
                            break;

                        case MoveDirection.Left:
                            dir = "<";
                            break;

                        default:
                            dir = ">";
                            break;
                    }
                    return string.Format("{0}{1}{2}{3}", NumMoved, Coord, dir, string.Join("",Drops));

                default:
                    return "???";
            }
        }

        /// <summary>
        /// The location of the move in Letter/Number notation
        /// 
        /// e.g. a1, h5
        /// </summary>
        public string Coord
        {
            get
            {
                return string.Format("{0}{1}", (char)('a' + Column), Row + 1);
            }
        }
        #endregion
        #region Legality Checking
        public bool IsLegal(GameState game)
        {
            string s;
            return IsLegal(game, out s);
        }

        /// <summary>
        /// Is this a legal move for the state of the game?
        /// </summary>
        /// <param name="boardState"></param>
        /// <returns></returns>
        public bool IsLegal(GameState game, out string reason)
        {
            TakBoard boardState = game.Board;

            if (Row < 0 || Row >= boardState.Size || Column<0 || Column>=boardState.Size)
            {
                reason = "Invalid coordinate";
                return false;
            }

            if (Type == MoveType.Place)
                return IsLegalPlace(game, out reason);
            else
                return IsLegalMove(game, out reason);
        }

        private bool IsLegalPlace(GameState game, out string reason)
        {
            TakBoard boardState = game.Board;

            if(game.TurnNumber == 0 && (Piece == TakPiece.PieceType.Capstone || Piece == TakPiece.PieceType.Wall))
            {
                reason = "Opening move must be a flat";
                return false;
            }
            else if(Piece == TakPiece.PieceType.Capstone && game[PieceColor].NumCapstones <= 0)
            {
                reason = "Out of capstones";
                return false;
            }
            else if(Piece != TakPiece.PieceType.Capstone && game[PieceColor].NumPieces <= 0)
            {
                reason = "Out of stones";
                return false;
            }
            else if (boardState[Row, Column].Size == 0)
            {
                reason = "";
                return true;
            }
            else
            {
                reason = "Cannot place a piece on a stack";
                return false;
            }
        }

        private bool IsLegalMove(GameState game, out string reason)
        {
            TakBoard boardState = game.Board;

            // if we're moving more pices than are in the stack
            if (boardState[Row, Column].Size < NumMoved || boardState[Row,Column].Size == 0)
            {
                reason = "Not enough pieces in stack";
                return false;
            }

            // we actually own the stack being moved
            if (boardState[Row, Column].Top.Color != PieceColor)
            {
                reason = "Stack belongs to another player";
                return false;
            }

            // if we're moving more pieces than the carry limit
            if (boardState.Size < NumMoved)
            {
                reason = "Carry limit exceeded";
                return false;
            }

            // if we're moving so far that we'd fall off the edge of the board
            if ((Direction == MoveDirection.Up && Row + Drops.Count >= boardState.Size) ||
                (Direction == MoveDirection.Down && Row - Drops.Count < 0) ||
                (Direction == MoveDirection.Right && Column + Drops.Count >= boardState.Size) ||
                (Direction == MoveDirection.Left && Column - Drops.Count < 0))
            {
                reason = "Not enough room to move in that direction";
                return false;
            }

            // if we're placing non-capstones on walls, or any piece on a capstone
            int dR, dC;
            if (Direction == MoveDirection.Left)
            {
                dR = 0;
                dC = -1;
            }
            else if (Direction == MoveDirection.Right)
            {
                dR = 0;
                dC = 1;
            }
            else if (Direction == MoveDirection.Up)
            {
                dR = 1;
                dC = 0;
            }
            else
            {
                dR = -1;
                dC = 0;
            }
            PieceStack src = boardState[Row, Column].NonDestructiveGrab(NumMoved);
            for (int i = 0; i < Drops.Count; i++)
            {
                int row = Row + dR * (i + 1);
                int col = Column + dC * (i + 1);
                PieceStack dst = boardState[row, col];

                if (!src.CanDrop(dst, Drops[i]))
                {
                    reason = string.Format("Cannot drop piece on {0}{1}", (char)(col + 'a'), row + 1);
                    return false;
                }

                src.Drop(Drops[i]);
            }

            // if we reach here then the move is legal
            reason = "";
            return true;
        }
        #endregion
        #region Actually doing the move
        /// <summary>
        /// Apply the move to the board
        /// 
        /// This does NOT do any legality checking; we assume that's been done already
        /// </summary>
        /// <param name="boardState"></param>
        public void Apply(GameState game)
        {
            if (Type == MoveType.Place)
                ApplyPlace(game);
            else
                ApplyMove(game);

            // auto-increment the game turn every time the black player makes a move
            if ((PieceColor == TakPiece.PieceColor.Black && game.TurnNumber > 0) ||
                (PieceColor == TakPiece.PieceColor.White && game.TurnNumber == 0))
                game.TurnNumber++;
        }

        private void ApplyPlace(GameState game)
        {
            TakPiece piece;
            if (Piece == TakPiece.PieceType.Capstone)
            {
                game[PieceColor].NumCapstones--;
                piece = new Capstone(PieceColor);
            }
            else
            {
                game[PieceColor].NumPieces--;
                piece = new TakPiece(PieceColor) { IsWall = Piece == TakPiece.PieceType.Wall };
            }

            game.Board[Row, Column].Place(piece);
        }

        private void ApplyMove(GameState game)
        {
            int dR, dC;
            if (Direction == MoveDirection.Left)
            {
                dR = 0;
                dC = -1;
            }
            else if (Direction == MoveDirection.Right)
            {
                dR = 0;
                dC = 1;
            }
            else if (Direction == MoveDirection.Up)
            {
                dR = 1;
                dC = 0;
            }
            else
            {
                dR = -1;
                dC = 0;
            }
            PieceStack stack = game.Board[Row, Column].Grab(NumMoved);
            for(int i=0; i<Drops.Count; i++)
            {
                int row = Row + dR * (i + 1);
                int col = Column + dC * (i + 1);
                PieceStack dst = game.Board[row, col];
                stack.Drop(dst, Drops[i]);
            }
        }
        
        public bool CheckAndApply(GameState game, out string reason)
        {
            bool ok = IsLegal(game, out reason);

            if (ok)
            {
                Apply(game);
            }
            return ok;
        }

        public bool CheckAndApply(GameState game)
        {
            string reason;
            return CheckAndApply(game, out reason);
        }
        #endregion
    }
}
