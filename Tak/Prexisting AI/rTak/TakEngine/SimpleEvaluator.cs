using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakEngine
{
    public class SimpleEvaluator : IEvaluator
    {
        UInt64[] _roadbitsx = new UInt64[2];
        UInt64[] _roadbitsy = new UInt64[2];
        int[] _roadScores = new int[2];
        int _size;

        public SimpleEvaluator(int size)
        {
            _size = size;
        }

        public string Name { get { return "SimpleEvaluator"; } }

        public void Evaluate(GameState game, out int eval, out bool gameOver)
        {
            _roadbitsx[0] = _roadbitsy[0] = _roadbitsx[1] = _roadbitsy[1] = 0;
            eval = 0;
            int score = 0;
            gameOver = false;
            bool foundEmpty = false;
            for (int x = 0; x < game.Size; x++)
                for (int y = 0; y < game.Size; y++)
                {
                    var stack = game.Board[x, y];
                    for (int j = 0; j < stack.Count; j++)
                    {
                        var piece = stack[j];
                        int pts = 0;
                        var stone = Piece.GetStone(piece);
                        if (stone == Piece.Stone_Flat)
                            pts = 2;
                        var player = Piece.GetPlayerID(piece);
                        if (player != 0)
                            pts *= -1;
                        if (j == stack.Count - 1)
                        {
                            score += pts;
                            pts *= 10;
                            if (stone != Piece.Stone_Standing)
                            {
                                var roadyoffset = y * game.Size + x;
                                var roadxoffset = x * game.Size + y;
                                _roadbitsx[player] |= 1ul << roadxoffset;
                                _roadbitsy[player] |= 1ul << roadyoffset;
                            }
                        }
                        else if (player == Piece.GetPlayerID(stack[stack.Count - 1]))
                            pts *= 2;
                        eval += pts;
                    }
                    if (stack.Count == 0)
                        foundEmpty = true;
                }

            for (int i = 0; i < 2; i++)
                _roadScores[i] = Math.Max(
                    RoadCheck(_roadbitsx[i]) ? 9999 - Math.Min(500, game.Ply) : 0,
                    RoadCheck(_roadbitsy[i]) ? 9999 - Math.Min(500, game.Ply) : 0);

            int roadScore = _roadScores[0] - _roadScores[1];
            if (roadScore != 0)
            {
                eval = roadScore;
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

        bool RoadCheck(UInt64 bits)
        {
            UInt64 mask = (1ul << _size) - 1ul;
            UInt64 row = bits & mask;
            for (int i = 1; i < _size; i++)
            {
                var next = (bits >> (_size * i)) & mask;
                row &= next;
                if (row == 0)
                    return false;
                UInt64 last;
                do
                {
                    last = row;
                    row |= next & ((row >> 1) | (row << 1));
                } while (row != last);
            } return row != 0;
        }
    }
}
