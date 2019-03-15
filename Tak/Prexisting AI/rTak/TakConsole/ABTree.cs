using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakEngine;


//implements an alpha-beta search for use in tak. Uses much of the same code as the MCTS implementation
namespace TakConsole
{
    public class ABTree
    {
        ABNode root;

        public ABTree(GameState game)
        {
            root = new ABNode(true, game);
        }

        List<IMove> moveOptions(GameState game)
        {
            //built-in function that is used to list available moves (maxDepth is irrelivant)
            var ai = new TakAI_V4(game.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;

            var legalMoves = new List<IMove>();
            Helper.EnumerateMoves(legalMoves, game, ai.RandomPositions);
            return legalMoves;
        }

        //uses same methodology and code as MCTree varient
        public float evaluate(ABNode node)
        {
            float positionValue = 0;
            int flatCount = 0;
            int highestCap = 0;
            int maxCap = 0, maxCapEnemy = 0;
            int losingState = 0;

            //Set coefficient values here for ease of parameter tuning
            float flatCo = 1F;
            float capCo = 5F;
            float lossCo = 1000F;

            //check if position is one move away from an enemy win state
            List<IMove> moves = moveOptions(node.state);
            var ai = new TakAI_V4(root.state.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;
            bool gameOver;
            int eval;
            TakEngine.Notation.MoveNotation notated;
            GameState state = node.state.DeepCopy();

            foreach (var m in moves)
            {
                TakEngine.Notation.MoveNotation.TryParse(m.Notate(), out notated);
                var match = notated.MatchLegalMove(moves);
                match.MakeMove(state);
                state.Ply++;
                evaluator.Evaluate(state, out eval, out gameOver);
                if (gameOver)
                    losingState = 1;
                m.TakeBackMove(state);
            }

            //count the number of flat stones
            for (int i = 0; i < node.state.Size; i++)
            {
                for (int j = 0; j < node.state.Size; j++)
                {
                    if (node.state.Board[i, j].Count > 0)
                    {
                        var piece = node.state.Board[i, j][node.state.Board[i, j].Count - 1];
                        if (Piece.GetStone(piece) == Piece.Stone_Flat)
                        {
                            if ((Piece.GetPlayerID(piece) == 1 && !node.player) || (Piece.GetPlayerID(piece) == 0 && node.player))
                            {
                                flatCount++;
                            }
                            else
                            {
                                //Account for enemy flat stones, making the heuristic a differential rather than a simple count
                                flatCount--;
                            }
                        }

                        //check for highest capstone
                        if (Piece.GetStone(piece) == Piece.Stone_Cap)
                        {
                            if (Piece.GetPlayerID(piece) == 1 && !node.player)
                            {
                                maxCapEnemy = node.state.Board[i, j].Count;
                            }
                            else if (Piece.GetPlayerID(piece) == 0 && node.player)
                            {
                                maxCap = node.state.Board[i, j].Count;
                            }
                        }
                    }
                }
            }

            //check which (if either) player has the highest capstone
            if (maxCap > maxCapEnemy)
                highestCap = 1;
            else if (maxCap < maxCapEnemy)
                highestCap = -1;

            positionValue = flatCount * flatCo + highestCap * capCo + losingState * lossCo;
            return positionValue;
        }

        public float ABSearch(ABNode node, int ply, int depth)
        {
            if (ply > depth)
            {
                return evaluate(node);
            }
            List<IMove> moves = moveOptions(root.state);
            List<float> evaluations = new List<float>();
            var ai = new TakAI_V4(root.state.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;
            bool gameOver;
            int eval;
            foreach (IMove m in moves)
            {
                ABNode n = new ABNode(!node.player, node.state);
                TakEngine.Notation.MoveNotation notated;
                TakEngine.Notation.MoveNotation.TryParse(m.Notate(), out notated);
                var match = notated.MatchLegalMove(moves);
                /* match.MakeMove(n.state);
                 node.state.Ply++;

                 evaluator.Evaluate(n.state, out eval, out gameOver);
                 if (gameOver)
                 {
                     if (node.player)
                         return 1000F;
                     else
                         return -1000F;
                 }*/

                evaluations.Add(ABSearch(n, ply + 1, depth));
            }

            if (node.player)
                return evaluations.Max();
            else
                return evaluations.Min();
        }

        public string AB(int depth)
        {
            List<IMove> moves = moveOptions(root.state);
            List<float> evaluations = new List<float>();
            var ai = new TakAI_V4(root.state.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;
            bool gameOver;
            int eval;
            Console.WriteLine(moves.Count);
            foreach (IMove m in moves)
            {
                GameState tempState = root.state.DeepCopy();
                ABNode n = new ABNode(!root.player, tempState);
                TakEngine.Notation.MoveNotation notated;
                TakEngine.Notation.MoveNotation.TryParse(m.Notate(), out notated);
                var match = notated.MatchLegalMove(moves);
                match.MakeMove(n.state);
                tempState.Ply++;

                evaluator.Evaluate(root.state, out eval, out gameOver);
                if (gameOver)
                {
                    return m.Notate();
                }
                Console.WriteLine("start");
                evaluations.Add(ABSearch((new ABNode(!root.player, tempState)), 0, depth));
                Console.WriteLine("end");
            }
            

            //get argMax of list
            int i = 0;
            int arg = 0;
            float max = 0;
            foreach (float f in evaluations)
            {
                if (f > max)
                {
                    max = f;
                    arg = i;
                }
                i++;
            }
            return moves[arg].Notate();
        }
    }

    public class ABNode
    {
        public GameState state;
        public List<ABNode> children = new List<ABNode>();
        public List<IMove> legalMoves = new List<IMove>();
        public ABNode parent;
        public bool player;
        float eval;

        public ABNode(bool turn, GameState game)
        {
            player = turn;
            state = game.DeepCopy();
        }
    }
}
