using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakEngine;

namespace TakConsole
{
    class MCTree
    {
        public MCNode root;
        int eval = 0;
        bool gameOver;

        public MCTree(GameState game)
        {
            GameState state = game.DeepCopy();
            root = new MCNode(true, state);
            Expand(root);
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

        //performs MCTS selection step (do not call if an available move is a win state)
        MCNode Selection()
        {
            var ai = new TakAI_V4(root.state.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;

            //random selection for testing purposes
            Random r = new Random();
            int index = r.Next(root.children.Count);

            MCNode iterator = root.children[index];
            List<MCNode> iteratorChildren = iterator.children;

            //looks for unexpanded node
            while (iterator.children.Count > 0)
            {
                bool gameOver;
                int eval;   //irrelivant, only exists to call evaluate function to look for gameover states

                //random
                int iteratorIndex = r.Next(iteratorChildren.Count);

                //avoid repeating pre-evaluated win states
                evaluator.Evaluate(iteratorChildren[iteratorIndex].state, out eval, out gameOver);
                if (!gameOver)
                {
                    iterator = iteratorChildren[iteratorIndex];
                    iteratorChildren = iterator.children;
                }
                else
                {
                    //avoids using the same value twice
                    iteratorChildren.RemoveAt(iteratorIndex);
                }
                    
            }
            
            return iterator;
        }

        //enumerates all available moves at a given node
        void Expand(MCNode leaf)
        {
            var ai = new TakAI_V4(root.state.Size, maxDepth: 4);
            var evaluator = ai.Evaluator;

            List<IMove> moves = moveOptions(leaf.state);
            
            TakEngine.Notation.MoveNotation notated;
            foreach (var m in moves)
            {
                GameState tempState = leaf.state.DeepCopy();
                TakEngine.Notation.MoveNotation.TryParse(m.Notate(), out notated);
                var match = notated.MatchLegalMove(moves);
                match.MakeMove(tempState);
                tempState.Ply++;
                leaf.children.Add(new MCNode(!leaf.player, tempState, leaf));
                leaf.legalMoves.Add(m);
                evaluator.Evaluate(leaf.children[leaf.children.Count - 1].state, out eval, out gameOver);
                if (gameOver)
                {
                    backProp(leaf.children[leaf.children.Count - 1], !leaf.player);
                }
            }
        }

        bool simulate(MCNode start)
        {
            int eval = 0;
            bool gameOver;
            var ai = new TakAI_V4(start.state.Size, maxDepth: 3);
            var evaluator = ai.Evaluator;
            List<IMove> moves = moveOptions(start.state);
            Expand(start);

            //check for end of game
            evaluator.Evaluate(start.state, out eval, out gameOver);
            if (gameOver)
                return !start.player;

            List<float> evaluations = new List<float>();
            //random play
            for (int i = 0; i < moves.Count; i++ )
            {
                MCNode m = start.children[i];
                float f = evaluate(m);
                //Console.WriteLine(f);
                if (f >= 0F)
                    evaluations.Add(f);
            }

            if (evaluations.Count == 0)
                return false;

            float sum = evaluations.Sum();

            Random r = new Random();
            double selectValue = r.NextDouble();
            float selection = 0F;
            int moveIndex = 0;

            while (selection < selectValue)
            {
                //Console.WriteLine(evaluations.Count.ToString() + "," + moveIndex.ToString());
                selection += evaluations[moveIndex] / sum;
                moveIndex++;
            }
            
            MCNode tempNode = new MCNode(!start.player, start.state.DeepCopy());
            TakEngine.Notation.MoveNotation notated;
            //Console.WriteLine(moveIndex.ToString() + "," + evaluations.Count.ToString() + "," + moves.Count.ToString());
            TakEngine.Notation.MoveNotation.TryParse(moves[moveIndex - 1].Notate(), out notated);
            var match = notated.MatchLegalMove(moves);
            match.MakeMove(tempNode.state);
            tempNode.state.Ply++;

            return simulate(tempNode);
        }

        //recursively updates each node from the leaf to the root the success or failure of a given path
        void backProp(MCNode node, bool winner)
        {
            while (node.parent != null)
            {
                if (winner)
                {
                    node.wins = node.wins + 1;
                }
                node.plays++;
                return;
            }
            return;
        }

        //uses above functions to perform an n-play MTCS
        public string MCTS(int n)
        {
            for (int i = 0; i < n; i++)
            {
                MCNode chosen = Selection();
                Expand(chosen);
                bool result = simulate(chosen);
                backProp(chosen, result);
            }

            float maxWins = 0F;
            int winIndex = 0;
            float maxPlays = 0;

            var ai = new TakAI_V4(root.state.Size, maxDepth: 0);
            var evaluator = ai.Evaluator;
            bool gameOver;
            int eval;

            //chooses highest win rate (or winning move if available)
            for (int i = 0; i < root.children.Count; i++)
            {
                evaluator.Evaluate(root.children[i].state, out eval, out gameOver);
                if (gameOver)
                {
                    return root.legalMoves[i].Notate();
                }
                if (root.children[i].winRate() > maxWins)
                {
                    if (root.children[i].plays > maxPlays)
                        maxPlays = root.children[i].plays;
                    maxWins = root.children[i].wins;
                    winIndex = i;
                }
            }

            return root.legalMoves[winIndex].Notate();
        }

        public float evaluate(MCNode node)
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

        public bool changeRoot(GameState newState)
        {
            foreach (MCNode c in root.children)
            {
                if (c.state.Board == newState.Board)
                {
                    root = c;
                    deleteParents(root);
                    return true;
                }
            }
            return false;
        }

        void deleteParents(MCNode newRoot)
        {
            if (newRoot.parent.parent != null)
                deleteParents(newRoot.parent);
            newRoot.parent = null;
            return;
        }
    }

    class MCNode
    {
        public GameState state;
        public List<MCNode> children = new List<MCNode>();
        public List<IMove> legalMoves = new List<IMove>();
        public MCNode parent;
        public float wins, plays;
        public bool player;

        public MCNode(bool turn, GameState game, MCNode parentNode = null)
        {
            parent = parentNode;
            player = turn;

            state = game.DeepCopy();
            wins = plays = 0;
        }

        public float winRate()
        {
            return wins / plays;
        }
    }
}

