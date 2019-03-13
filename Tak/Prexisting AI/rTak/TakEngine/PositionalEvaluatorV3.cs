using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakEngine
{
    public class PositionalEvaluatorV3 : IEvaluator
    {
        FloodFill _flood = new FloodFill();
        int[,] _ids;
        UInt64[] _inf = new ulong[2];

        int _fillingid = 0;
        int _fillContextPlayer;
        GameState _fillContextState;
        int[] _roadScores = new int[2];
        int _boardSize;

        public string Name { get { return "PositionalEvaluatorV3"; } }

        public PositionalEvaluatorV3(int boardSize)
        {
            _ids = new int[boardSize, boardSize];
            _boardSize = boardSize;

            // precalculate bit masks for bitwise floodfill
            _expMaskX0 = _expMaskX1 = _expMaskY0 = _expMaskY1 = 0ul;
            for (int y = 0; y < _boardSize; y++)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    UInt64 bit = 1ul << (y * _boardSize + x);
                    if (x > 0)
                        _expMaskX0 |= bit;
                    if (y > 0)
                        _expMaskY0 |= bit;
                    if (x < _boardSize - 1)
                        _expMaskX1 |= bit;
                    if (y < _boardSize - 1)
                        _expMaskY1 |= bit;
                }
            }
        }

        BoardPosition _fillmin, _fillmax;

        /// <summary>
        /// Evaluate the current board position
        /// </summary>
        /// <param name="game">Current game state</param>
        /// <param name="eval">Evaluation function output.  Positive values favor the first player while negative values favor the second player.</param>
        /// <param name="gameOver">Indicates if any game ending condition has been reached</param>
        public void Evaluate(GameState game, out int eval, out bool gameOver)
        {
            gameOver = false;
            _fillContextState = game;
            _inf[0] = 0;
            _inf[1] = 0;

            // reset island id numbers
            for (int i = 0; i < game.Size; i++)
                for (int j = 0; j < game.Size; j++)
                {
                    _ids[i, j] = 0;
                    var offset = i * _boardSize + j;
                }

            // Ensure every flat stone / cap stone on the edges has been assigned an island ID number
            _roadScores[0] = _roadScores[1] = 0;
            eval = 0;
            for (int x = 0; x < game.Size; x++)
                for (int y = 0; y < game.Size; y++)
                {
                    var piece = game[x, y];
                    if (piece.HasValue && _ids[x, y] == 0 && Piece.GetStone(piece.Value) != Piece.Stone_Standing)
                    {
                        _fillingid++;
                        _fillContextPlayer = Piece.GetPlayerID(piece.Value);

                        _fillmax = _fillmin = new BoardPosition(x, y);
                        _flood.Fill(x, y, doesMatch, paint);

                        if ((_fillmin.X == 0 && _fillmax.X == _boardSize - 1) ||
                            (_fillmin.Y == 0 && _fillmax.Y == _boardSize - 1))
                            _roadScores[_fillContextPlayer] = 9999 - Math.Min(500, game.Ply);
                        eval += ((Math.Abs(_fillmax.X - _fillmin.X) + Math.Abs(_fillmax.Y - _fillmin.Y)) * (_fillContextPlayer * -2 + 1)) * 3;
                    }
                }

            int score = _roadScores[0] - _roadScores[1];
            if (score != 0)
            {
                eval = score;
                gameOver = true;
                return;
            }
            else if (_roadScores[0] > 0 && (game.Ply & 1) == 1)
            {
                eval = _roadScores[0];
                gameOver = true;
                return;
            }
            else if (_roadScores[1] > 0 && (game.Ply & 1) == 0)
            {
                eval = -_roadScores[1];
                gameOver = true;
                return;
            }
            bool foundEmpty = false;
            for (int y = 0; y < _boardSize; y++)
                for (int x = 0; x < _boardSize; x++)
                {
                    var stack = game.Board[x, y];
                    for (int j = 0; j < stack.Count; j++)
                    {
                        var piece = stack[j];
                        int pts = 0;
                        if (Piece.GetStone(piece) == Piece.Stone_Flat)
                            pts = 2;
                        var player = Piece.GetPlayerID(piece);
                        if (player != 0)
                            pts *= -1;
                        if (j == stack.Count - 1)
                        {
                            score += pts;
                            pts *= 10;

                            _inf[player] |= 1ul << (y * _boardSize + x);
                        }
                        else if (player == Piece.GetPlayerID(stack[stack.Count - 1]))
                            pts *= 2;
                        eval += pts;
                    }
                    if (stack.Count == 0)
                        foundEmpty = true;
                }

            while (true)
            {
                var expanded0 = Expand(_inf[0]);
                var expanded1 = Expand(_inf[1]);
                var intersection = expanded0 & expanded1;
                expanded0 &= ~intersection;
                expanded1 &= ~intersection;
                if ((expanded0 & ~_inf[0]) != 0ul ||
                    (expanded1 & ~_inf[1]) != 0ul)
                {
                    _inf[0] |= expanded0;
                    _inf[1] |= expanded1;
                }
                else
                    break;
            }

            eval += CountBits(_inf[0]);
            eval -= CountBits(_inf[1]);

            if (!foundEmpty ||
                (game.StonesRemaining[0] + game.CapRemaining[0]) == 0 ||
                (game.StonesRemaining[1] + game.CapRemaining[1]) == 0)
            {
                gameOver = true;
                if (score > 0)
                    eval = Evaluation.FlatWinEval;
                else if (score < 0)
                    eval = -Evaluation.FlatWinEval;
                else
                    eval = 0;
            }
        }

        UInt64 _expMaskX0, _expMaskX1, _expMaskY0, _expMaskY1;
        UInt64 Expand(UInt64 bits)
        {
            return bits |
                ((bits & _expMaskX0) >> 1) |
                ((bits & _expMaskX1) << 1) |
                ((bits & _expMaskY0) >> _boardSize) |
                ((bits & _expMaskY1) << _boardSize);
        }

        int CountBits(UInt64 bits)
        {
            int count = 0;
            while (bits != 0ul)
            {
                count += (int)bits & 1;
                bits >>= 1;
            }
            return count;
        }

        bool doesMatch(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _boardSize || y >= _boardSize)
                return false;
            if (_ids[x, y] != 0)
                return false;
            var piece = _fillContextState[x, y];
            if (!piece.HasValue)
                return false;
            if (Piece.GetPlayerID(piece.Value) != _fillContextPlayer)
                return false;
            if (Piece.GetStone(piece.Value) == Piece.Stone_Standing)
                return false;
            return true;
        }

        void paint(int x, int y)
        {
            _ids[x, y] = _fillingid;
            _fillmin.X = Math.Min(x, _fillmin.X);
            _fillmin.Y = Math.Min(y, _fillmin.Y);
            _fillmax.X = Math.Max(x, _fillmax.X);
            _fillmax.Y = Math.Max(y, _fillmax.Y);
        }
    }
}
