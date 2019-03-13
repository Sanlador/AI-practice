namespace TakEngine
{
    public interface ITakAI
    {
        IMove FindGoodMove(GameState game);
        int MaxDepth { get; set; }
        BoardPosition[] NormalPositions { get; }
        void Cancel();
        bool Canceled { get; }
        IEvaluator Evaluator { get; set; }
    }
}
