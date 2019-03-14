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
        MCNode root;
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

            //check for end of game
            evaluator.Evaluate(start.state, out eval, out gameOver);
            if (gameOver)
                return !start.player;

            //random play
            Random r = new Random();
            int moveIndex = r.Next(moves.Count);
            MCNode tempNode = start;
            tempNode.player = !tempNode.player;
            tempNode.state = start.state.DeepCopy();

            TakEngine.Notation.MoveNotation notated;
            TakEngine.Notation.MoveNotation.TryParse(moves[moveIndex].Notate(), out notated);
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
            for(int i = 0; i < n; i++)
            {
                MCNode chosen = Selection();
                Expand(chosen);
                bool result = simulate(chosen);
                backProp(chosen, result);
            }

            float maxWins = 0F;
            int winIndex = 0;
            float maxPlays = 0;

            //chooses highest win rate (still under construction)
            for(int i = 0; i < root.legalMoves.Count; i++)
            {
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

        public int evaluate(MCNode node)
        {
            GameState state = node.state.DeepCopy();
            //bool[,] checkedSpaces = new bool[state.Size, state.Size];
            int len = 0;
            
            //Find the longest road made by the current player
            //still under construction
            /*
            for (int i = 0; i < state.Size; i++)
            {
                for (int j = 0; j < state.Size; j++)
                {
                    var piece = state.Board[i, j][state.Board[i, j].Count - 1];
                    if (Piece.GetStone(piece) == Piece.Stone_Flat)
                    {
                        if ((Piece.GetPlayerID(piece) == 1 && !node.player) || (Piece.GetPlayerID(piece) == 0 && node.player))
                        {
                            
                        }
                    }
                }
            }*/

            return 0;
        }

        bool changeRoot(GameState newState)
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

