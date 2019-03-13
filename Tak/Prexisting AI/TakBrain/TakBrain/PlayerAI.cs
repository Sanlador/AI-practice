using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// An AI that chooses moves for Tak
    /// </summary>
    public class PlayerAI
    {
        /// <summary>
        /// Create an AI to control the given pieces on the board
        /// </summary>
        /// <param name="ownColor"></param>
        /// <param name="sharedBoard"></param>
        public PlayerAI(TakPiece.PieceColor ownColor, GameState sharedGame)
        {
            MyColor = ownColor;
            TheirColor = ownColor == TakPiece.PieceColor.White ? TakPiece.PieceColor.Black : TakPiece.PieceColor.White;
            Game = sharedGame;
        }

        private TakPiece.PieceColor MyColor { get; set; }
        private TakPiece.PieceColor TheirColor { get; set; }
        private GameState Game { get; set; }
        private static Random rnd = new Random();

        /// <summary>
        /// Enumerate all legal moves possible for this player, given the state of the game
        /// 
        /// Returned moves are unsorted (and are in fact shuffles to avoid any biases)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="boardState"></param>
        /// <returns></returns>
        public static List<TakMove> EnumerateMoves(TakPiece.PieceColor player, GameState gameState)
        {
            TakMove m;
            List<TakMove> moves = new List<TakMove>();
            TakPiece.PieceColor otherPlayer = player == TakPiece.PieceColor.Black ? TakPiece.PieceColor.White : TakPiece.PieceColor.Black;

            // first off, we can drop pieces on any open space on the board
            // if we're still evaluating moves it must mean we still have at least 1 piece to place
            for(int i=0; i< gameState.Board.Size; i++)
            {
                for(int j=0; j< gameState.Board.Size; j++)
                {
                    if(gameState.Board[i,j].Size == 0)
                    {
                        // from turn 1 on we place our own pieces and can place flats, capstones, or walls
                        if (gameState.TurnNumber > 0)
                        {
                            m = new TakMove(player, i, j, TakPiece.PieceType.Flat);
                            moves.Add(m);
                            
                            m = new TakMove(player, i, j, TakPiece.PieceType.Wall);
                            moves.Add(m);

                            if (gameState[player].NumCapstones > 0)
                            {
                                m = new TakMove(player, i, j, TakPiece.PieceType.Capstone);
                                moves.Add(m);
                            }
                        }
                        else
                        {
                            // on turn 0 we can only place an opposing flat
                            m = new TakMove(otherPlayer, i, j, TakPiece.PieceType.Flat);
                            moves.Add(m);
                        }
                    }
                }
            }

            // generate all possible moves for each stack we own
            // this is only allowed from turn 1 onward
            if (gameState.TurnNumber > 0)
            {
                for (int i = 0; i < gameState.Board.Size; i++)
                {
                    for (int j = 0; j < gameState.Board.Size; j++)
                    {
                        if (gameState.Board[i, j].Owner == player)
                        {
                            List<TakMove> stackMoves = GenerateStackMoves(player, gameState, i, j);

                            foreach (TakMove tm in stackMoves)
                                moves.Add(tm);
                        }
                    }
                }
            }

            moves.Shuffle();
            return moves;
        }

        /// <summary>
        /// Generate the list of all moves that the stack located at the given coordinate can make
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private static List<TakMove> GenerateStackMoves(TakPiece.PieceColor player, GameState gameState, int row, int column)
        {
            TakMove m;
            List<TakMove> moves = new List<TakMove>();
            PieceStack stack = gameState.Board[row, column];
            List<List<int>> allDrops = new List<List<int>>();

            for(int n = 1; n<=Math.Min(stack.Size, gameState.Board.Size); n++) // for 1 to the carry limit of the board
            {
                allDrops.Clear();
                MakeDrops(n, allDrops, new List<int>());

                foreach(List<int> drops in allDrops)
                {
                    m = new TakMove(player, row, column, TakMove.MoveDirection.Left, drops);
                    if (m.IsLegal(gameState))
                        moves.Add(m);

                    m = new TakMove(player, row, column, TakMove.MoveDirection.Right, drops);
                    if (m.IsLegal(gameState))
                        moves.Add(m);

                    m = new TakMove(player, row, column, TakMove.MoveDirection.Up, drops);
                    if (m.IsLegal(gameState))
                        moves.Add(m);
                    m = new TakMove(player, row, column, TakMove.MoveDirection.Down, drops);
                    if (m.IsLegal(gameState))
                        moves.Add(m);
                }
            }

            return moves;
        }

        /// <summary>
        /// Recursively the possible combinations of drops for a stack with n pieces remaining in-hand
        /// </summary>
        private static void MakeDrops(int num, List<List<int>> allDrops, List<int> current)
        {
            if(num <= 0)
            {
                // we've dropped all the pieces; add current to the list of all possible drop combinations
                List<int> d = new List<int>();
                foreach (int i in current)
                    d.Add(i);
                allDrops.Add(d);
            }
            else
            {
                for(int i=1; i<=num; i++)
                {
                    current.Add(i);
                    MakeDrops(num - i, allDrops, current);
                    current.RemoveAt(current.Count - 1);
                }
            }
        }

        public TakMove ChooseNextMove()
        {
            /*
            // for now just pick the "best" move
            List<ScoredMove> moves = EnumerateMoves(Game.TurnNumber == 0 ? TheirColor : MyColor, Game);

            // choose a random move that's tied with whatever our best score is
            double bestScore = moves[0].Score;
            int i;
            for (i = 1; i < moves.Count && moves[i].Score == bestScore; i++) { }
            return moves[rnd.Next(i)].Move;
            */

            MoveTree tree = new MoveTree(MyColor, Game);
            tree.Expand(3);
            return tree.BestMove;
        }
    }
}
