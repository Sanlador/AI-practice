using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakBrain
{
    /// <summary>
    /// Dynamically-expanding tree structure that explores and ranks possible moves
    /// </summary>
    internal class MoveTree
    {
        /// <summary>
        /// A single node in the tree
        /// </summary>
        private abstract class TreeNode
        {
            protected TreeNode()
            {
                Children = new List<TakBrain.MoveTree.MoveNode>();
                Depth = 0;
            }

            /// <summary>
            /// The depth of the node in the tree
            /// </summary>
            public int Depth { get; set; }

            public List<MoveNode> Children { get; set; }
        }

        private class RootNode : TreeNode
        {
            public RootNode(GameState state) : base()
            {
                InitialState = state;
            }

            /// <summary>
            /// The initial state of the game 
            /// </summary>
            public GameState InitialState { get; set; }
        }

        private class MoveNode : TreeNode, IComparable<MoveNode>
        {
            public MoveNode(GameState gs, TakMove move) : base()
            {
                Move = move;
                ResultingState = new GameState(gs);
                move.Apply(ResultingState);
            }

            /// <summary>
            /// The move that leads us from the parent node to this node
            /// </summary>
            public TakMove Move { get; set; }

            /// <summary>
            /// The highest score of this node's children
            /// 
            /// (Or this move's own score if this is a leaf node)
            /// </summary>
            public double Score
            {
                get; set;
            }

            public GameState ResultingState { get; set; }

            public MoveNode Parent { get; set; }

            /// <summary>
            /// Sort based on ascending score
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(MoveNode other)
            {
                int sort = Score.CompareTo(other.Score);
                return sort;
            }

            public override string ToString()
            {
                return string.Format("{0:00}. {1}", ResultingState.TurnNumber, Move);
            }
        }

        private RootNode Root;
        private TakPiece.PieceColor MyColor;
        private TakPiece.PieceColor TheirColor;
        private int MaxDepth;
        private int NumLeaves;
        private List<TakBoard> ConsideredBoardStates;

        public MoveTree(TakPiece.PieceColor player, GameState state)
        {
            MyColor = player;
            TheirColor = player == TakPiece.PieceColor.Black ? TakPiece.PieceColor.White : TakPiece.PieceColor.Black;
            Root = new RootNode(state);
        }

        /// <summary>
        /// Expand the tree out to our maximum depth, only expanding leaf nodes with the best-possible score
        /// </summary>
        /// <param name="maxDepth"></param>
        public void Expand(int maxDepth)
        {
            NumLeaves = 0;
            ConsideredBoardStates = new List<TakBoard>();
            List<TakMove> nextMoves = PlayerAI.EnumerateMoves(MyColor, Root.InitialState);
            foreach (TakMove m in nextMoves)
            {
                MoveNode n = new MoveNode(Root.InitialState, m);
                if (!ConsideredBoardStates.Contains(n.ResultingState.Board))
                {
                    ConsideredBoardStates.Add(n.ResultingState.Board);
                    n.Depth = 1;
                    n.Parent = null;
                    n.Score = n.ResultingState.Evaluate(MyColor);
                    Root.Children.Add(n);
                }
            }

            // recursively expand the tree
            MaxDepth = maxDepth;
            foreach (MoveNode n in Root.Children)
            {
                n.Score = ABPrune(n, (double)int.MinValue - 1, (double)int.MaxValue+1, true);
            }
            Root.Children.Sort();
            PrintDebug(string.Format("Explored {0} board states", ConsideredBoardStates.Count));
            PrintDebug(string.Format("Explored {0} leaves", NumLeaves));
        }

        /// <summary>
        /// Expand a node using AB pruning
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private double ABPrune(MoveNode root, double alpha, double beta, bool isMaxing)
        {
            // don't re-expand if we hit an automatic win or loss, or if we're at the depth limit
            if (root.Depth >= MaxDepth || root.ResultingState.GameOver)
            {
                NumLeaves++;
                root.Score = root.ResultingState.Evaluate(MyColor);
                return root.Score;
            }
            else
            {
                List<TakMove> nextMoves = PlayerAI.EnumerateMoves(isMaxing ? MyColor : TheirColor, root.ResultingState);
                double v;
                if(isMaxing)
                {
                    v = (double)int.MinValue - 1;
                    foreach(TakMove m in nextMoves)
                    {
                        MoveNode n = new MoveNode(root.ResultingState, m);
                        n.Depth = root.Depth + 1;
                        root.Children.Add(n);
                        n.Parent = root;
                        if (!ConsideredBoardStates.Contains(n.ResultingState.Board))
                        {
                            //Console.WriteLine("Considering\n{0}", n.ResultingState.Board);
                            ConsideredBoardStates.Add(n.ResultingState.Board);

                            v = Math.Max(v, ABPrune(n, alpha, beta, false));
                            alpha = Math.Max(alpha, v);
                            if (beta <= alpha)
                                break;
                        }
                    }
                }
                else
                {
                    v = (double)int.MaxValue + 1;
                    foreach (TakMove m in nextMoves)
                    {
                        MoveNode n = new MoveNode(root.ResultingState, m);
                        n.Depth = root.Depth + 1;
                        root.Children.Add(n);
                        n.Parent = root;
                        if (!ConsideredBoardStates.Contains(n.ResultingState.Board))
                        {
                            //Console.WriteLine("Considering\n{0}", n.ResultingState.Board);
                            ConsideredBoardStates.Add(n.ResultingState.Board);

                            v = Math.Max(v, ABPrune(n, alpha, beta, true));
                            beta = Math.Min(beta, v);
                            if (beta <= alpha)
                                break;
                        }
                    }
                }

                root.Children.Sort();
                if (isMaxing)
                    root.Children.Reverse();

                return v;
            }
        }

        public TakMove BestMove
        {
            get
            {
                return Root.Children.Last().Move;
            }
        }

        private void PrintDebug(MoveNode n, bool toParent = true)
        {
            Action a = () =>
            {
                List<System.Windows.Controls.Label> labels = new List<System.Windows.Controls.Label>();

                MoveNode current = n;
                while (current != null)
                {
                    System.Windows.Controls.Label l = new System.Windows.Controls.Label();
                    l.Margin = new System.Windows.Thickness(current.Depth * 10, 0, 0, 0);
                    l.Content = string.Format("{0} :: {1}", current.ToString(), current.Score);
                    labels.Add(l);

                    if (toParent)
                        current = current.Parent;
                    else
                        current = null;
                }

                for (int i = labels.Count - 1; i >= 0; i--)
                {
                    MainWindow.DebugArea.Children.Add(labels[i]);
                }

                /*
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Margin = new System.Windows.Thickness(n.Depth * 10, 0, 0, 0);
                img.Source = MainWindow.Bitmap2Source(n.ResultingState.Board.Draw());
                img.Width = 128;
                MainWindow.DebugArea.Children.Add(img);
                */
            };
            MainWindow.DebugArea.Dispatcher.InvokeAsync(a);
        }

        private void PrintDebug(string s)
        {
            Action a = () =>
            {
                System.Windows.Controls.Label l = new System.Windows.Controls.Label();
                l.Content = s;
                MainWindow.DebugArea.Children.Add(l);
            };
            MainWindow.DebugArea.Dispatcher.InvokeAsync(a);
        }
    }
}
