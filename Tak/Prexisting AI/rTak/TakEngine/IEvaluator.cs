
namespace TakEngine
{
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluates the current board position
        /// </summary>
        /// <param name="game">Current game state</param>
        /// <param name="eval">Evaluation function output.  Positive values favor the first player.</param>
        /// <param name="gameOver">Indicates if any game ending condition has been reached</param>
        void Evaluate(GameState game, out int eval, out bool gameOver);

        /// <summary>
        /// Gets human-readable evaluation function name
        /// </summary>
        string Name { get; }
    }
}
