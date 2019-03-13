using System;
using System.Collections.Generic;

namespace TakEngine
{
    public class TakAI_V4 : ITakAI
    {
        /// <summary>
        /// Default value for the maximum game tree search depth
        /// </summary>
        const int DefaultMaxDepth = 3;
        BoardPosition[] _randomPositions;
        BoardPosition[] _normalPositions;

        /* Notes on maximum depth:
         * Higher values increase the strength of the AI but also make it much slower.
         * In the late game there can be more than 100 moves in a single board position, so looking 1 extra move into the
         * future can be 100x slower.  That is the worst case scenario, though, which is often mitigated by alpha-beta
         * pruning.  So in practice the AI does not get slower _quite_ that fast... usually... but it varies by board position and
         * some positions don't get much benefit from pruning.  There's also a bit of luck involved based on the order in which
         * the AI considers moves.
         */

        /// <summary>
        /// Maximum game tree search depth.
        /// </summary>
        int _maxDepth;

        /// <summary>
        /// Psuedo-random number generator is only used to randomize the order in which positions are considered.
        /// </summary>
        Random _rand;
        AIThreadData _singleThreadData;
        bool _canceled = false;
        bool _canceling = false;

        long _evals = 0;
        System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();
        static string DebugLogFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tak_ai_debug.txt");

        public BoardPosition[] RandomPositions { get { return _randomPositions; } }
        public BoardPosition[] NormalPositions { get { return _normalPositions; } }
        public int LastEvaluation { get; private set; }

        public TakAI_V4(int boardSize, int maxDepth = DefaultMaxDepth, IEvaluator evaluator = null)
        {
            _maxDepth = maxDepth;
            _rand = new Random();

            // Initialize list of all legal board positions
            _normalPositions = new BoardPosition[boardSize * boardSize];
            for (int i = 0; i < _normalPositions.Length; i++)
                _normalPositions[i] = new BoardPosition(i % boardSize, i / boardSize);

            // Copy board positions into an alternate array that can be randomized to make the AI more interesting
            _randomPositions = new BoardPosition[boardSize * boardSize];
            Array.Copy(_normalPositions, _randomPositions, _normalPositions.Length);

            if (evaluator == null)
                evaluator = new PositionalEvaluatorV3(boardSize);
            Evaluator = evaluator;
        }

        public void Cancel()
        {
            _canceling = true;
        }
        public bool Canceled { get { return _canceled; } }
        public IEvaluator Evaluator { get; set; }

        public IMove FindGoodMove(GameState game)
        {
            // Clear cancelation state before beginning to process the move
            _canceled = _canceling = false;

            _evals = 0;
            _timer.Stop();
            _timer.Reset();
            _timer.Restart();

            // Randomize order of board positions so that the AI appears less predictable
            // in the early phases of the game
            _randomPositions.RandomizeOrder(_rand);

            IMove bestMove = null;
            int bestEval;

            if (_singleThreadData == null)
                _singleThreadData = new AIThreadData(game.Size, _maxDepth, Evaluator);
            _singleThreadData.Reset(game);
            for (_deepenTo = 0; _deepenTo <= _maxDepth; _deepenTo++)
            {
                FindGoodMove(_singleThreadData, 0, null, out bestMove, out bestEval);
                if (Math.Abs(bestEval) >= Evaluation.FlatWinEval)
                    break;
                LastEvaluation = bestEval;
            }

            _timer.Stop();
            if (Properties.Settings.Default.debug)
            {
                System.IO.File.AppendAllText(DebugLogFilePath,
                    string.Format("{0}\t{1}\t{2:0.0000}\t{3:0.0000}\r\n",
                    Evaluator.Name,
                    _evals,
                    _timer.Elapsed.TotalSeconds,
                    (double)_evals / _timer.Elapsed.TotalSeconds));
            }

            return bestMove;
        }

        /// <summary>
        /// Gets or sets the AI's search tree depth (i.e. difficulty).
        /// NOTE: Depth N+1 is roughly 30 to 50 times slower than depth N.
        /// NOTE: The AI actually gets slower in the late game due to bigger stacks of stones
        /// </summary>
        public int MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                _maxDepth = value;
                _singleThreadData = null;
            }
        }

        int _deepenTo;
        void FindGoodMove(AIThreadData data, int depth, int? prune, out IMove bestMove, out int bestEval)
        {
            bestMove = null;
            bestEval = -999999;
            if (_canceling)
            {
                _canceled = true;
                return;
            }
            var moves = data.DepthMoves[depth];
            moves.Clear();

            var game = data.Game;
            Helper.EnumerateMoves(moves, game, _randomPositions);

            // on the very first ply we only need to consider moves in one quadrant of the board because of rotational symmetry
            if (data.Game.Ply == 0)
            {
                var maxcoord = (data.Game.Size + 1) >> 1;
                for (int i = 0; i < moves.Count; i++)
                {
                    var m = (PlacePieceMove)moves[i];
                    if (m.Pos.X >= maxcoord || m.Pos.Y >= maxcoord)
                    {
                        moves[i] = moves[moves.Count - 1];
                        moves.RemoveAt(moves.Count - 1);
                    }
                }
            }

            {
                int writeto = 0;
                if (data.KillerMoves1[depth] != null)
                {
                    var killer = data.KillerMoves1[depth];
                    for (int i = writeto + 1; i < moves.Count; i++)
                    {
                        if (moves[i].CompareTo(killer))
                        {
                            var swap = moves[writeto];
                            moves[writeto] = moves[i];
                            moves[i] = swap;
                            writeto++;
                            break;
                        }
                    }
                }
                if (data.KillerMoves2[depth] != null)
                {
                    var killer = data.KillerMoves2[depth];
                    for (int i = writeto + 1; i < moves.Count; i++)
                    {
                        if (moves[i].CompareTo(killer))
                        {
                            var swap = moves[writeto];
                            moves[writeto] = moves[i];
                            moves[i] = swap;
                            writeto++;
                            break;
                        }
                    }
                }
            }

            foreach (var move in moves)
            {
                move.MakeMove(game);
                game.Ply++;

                int eval;
                bool gameOver;
                if (depth != _deepenTo)
                    data.SimpleEvaluator.Evaluate(game, out eval, out gameOver);
                else
                    data.Evaluator.Evaluate(game, out eval, out gameOver);
                _evals++;
                if (0 == (game.Ply & 1))
                    eval *= -1;
                if (!(depth == _deepenTo || gameOver))
                {
                    IMove opmove;
                    int opeval;
                    FindGoodMove(data, depth + 1, bestMove == null ? (int?)null : -bestEval, out opmove, out opeval);
                    eval = opeval * -1;
                }
                if (eval > bestEval)
                {
                    bestMove = move;
                    bestEval = eval;

                    data.KillerMoves2[depth] = data.KillerMoves1[depth];
                    data.KillerMoves1[depth] = bestMove;
                }

                game.Ply--;
                move.TakeBackMove(game);

                if (gameOver && eval > 0)
                    break;
                if (prune.HasValue && eval >= prune.Value)
                    break;
            }
        }

        class AIThreadData
        {
            public IEvaluator Evaluator;
            public GameState Game;
            public List<List<IMove>> DepthMoves;
            public IMove[] KillerMoves1;
            public IMove[] KillerMoves2;
            public long Evals = 0;
            public SimpleEvaluator SimpleEvaluator;
            public AIThreadData(int size, int maxDepth, IEvaluator evaluator)
            {
                Evaluator = evaluator;
                DepthMoves = new List<List<IMove>>();
                for (int i = 0; i <= maxDepth; i++)
                    DepthMoves.Add(new List<IMove>());
                KillerMoves1 = new IMove[maxDepth + 1];
                KillerMoves2 = new IMove[maxDepth + 2];
                this.SimpleEvaluator = new SimpleEvaluator(size);
            }
            public void Reset(GameState game)
            {
                Game = game.DeepCopy();
                Array.Clear(KillerMoves1, 0, KillerMoves1.Length);
                Array.Clear(KillerMoves2, 0, KillerMoves2.Length);
            }
        }
    }
}
