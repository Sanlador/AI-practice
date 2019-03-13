using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TakBrain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IsAI.Add(TakPiece.PieceColor.Black, false);
            IsAI.Add(TakPiece.PieceColor.White, false);
            AIs.Add(TakPiece.PieceColor.Black, new PlayerAI(TakPiece.PieceColor.Black, new GameState(new TakBoard(3))));
            AIs.Add(TakPiece.PieceColor.White, new PlayerAI(TakPiece.PieceColor.White, new GameState(new TakBoard(3))));

            DebugArea = DebugTrace;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Is there currently a game going on?
        /// 
        /// Changing this will enable/disable portions of the GUI as needed
        /// 
        /// For simplicity, we use the chess convention where the light player always goes first
        /// </summary>
        private bool GameInProgress
        {
            get { return _gameInProgress; }
            set
            {
                _gameInProgress = value;
                if (_gameInProgress)
                {
                    btnStart.Content = "Reset";
                    chkBlackIsAI.IsEnabled = false;
                    chkWhiteIsAI.IsEnabled = false;

                    Board = new TakBoard(5);
                    Title = string.Format("TakBrain ({0}x{0})",Board.Size);
                    Game = new GameState(Board);
                    AIs[TakPiece.PieceColor.Black] = new PlayerAI(TakPiece.PieceColor.Black, Game);
                    AIs[TakPiece.PieceColor.White] = new PlayerAI(TakPiece.PieceColor.White, Game);

                    txtWhiteMoves.Text = string.Format("Board: {0}x{0}\n",Board.Size);
                    txtBlackMoves.Text = string.Format("Board: {0}x{0}\n", Board.Size); ;
                    txtWhiteMove.Text = "";
                    txtBlackMove.Text = "";
                    txtWhiteMove.IsEnabled = true;
                    txtBlackMove.IsEnabled = false;

                    CurrentPlayer = TakPiece.PieceColor.White;
                    imgBoard.Source = Bitmap2Source(Board.Draw());
                }
                else
                {
                    btnStart.Content = "Start Game";
                    chkBlackIsAI.IsEnabled = true;
                    chkWhiteIsAI.IsEnabled = true;
                    
                    txtWhiteMove.Text = "";
                    txtBlackMove.Text = "";
                    txtWhiteMoves.Text = "";
                    txtBlackMoves.Text = "";
                    txtWhiteMove.IsEnabled = false;
                    txtBlackMove.IsEnabled = false;
                    btnSubmit.IsEnabled = false;
                    imgBoard.Source = null;

                    Title = "TakBrain";
                }
            }
        }
        private bool _gameInProgress = false;

        /// <summary>
        /// The current player's colour
        /// </summary>
        private TakPiece.PieceColor CurrentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                _currentPlayer = value;

                switch(CurrentPlayer)
                {
                    case TakPiece.PieceColor.White:
                        txtWhiteMove.Text = "";
                        txtWhiteMove.IsEnabled = true;
                        txtBlackMove.IsEnabled = false;
                        ActiveInput = txtWhiteMove;
                        ActiveOutput = txtWhiteMoves;
                        break;

                    case TakPiece.PieceColor.Black:
                        txtBlackMove.Text = "";
                        txtBlackMove.IsEnabled = true;
                        txtWhiteMove.IsEnabled = false;
                        ActiveInput = txtBlackMove;
                        ActiveOutput = txtBlackMoves;
                        break;
                }

                if(IsAI[_currentPlayer])
                {
                    DebugArea.Children.Clear();
                    ActiveInput.IsEnabled = false;
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += RunAI_Async;
                    worker.RunWorkerAsync();
                }
                else
                {
                    ActiveInput.Focus();
                }
            }
        }

        private void RunAI_Async(object sender, DoWorkEventArgs e)
        {
            TakMove move = AIs[CurrentPlayer].ChooseNextMove();

            Action a = () =>
            {
                ActiveInput.Text = move.ToString();
                btnSubmit_Click(this, new RoutedEventArgs());
            };
            Dispatcher.InvokeAsync(a);
        }

        private TakPiece.PieceColor _currentPlayer;

        /// <summary>
        /// The textbox the user is typing their move into
        /// 
        /// One textbox exists on each side of the UI, and toggling <see cref="CurrentPlayer"/> 
        /// will assign which one is currently in-use
        /// </summary>
        private TextBox ActiveInput { get; set; }

        /// <summary>
        /// The textbox all the player's moves are displayed in
        /// 
        /// One textbox exists on each side of the UI, and toggling <see cref="CurrentPlayer"/> 
        /// will assign which one is currently in-use
        /// </summary>
        private TextBox ActiveOutput { get; set; }

        /// <summary>
        /// The actual live game board
        /// </summary>
        private TakBoard Board { get; set; }

        /// <summary>
        /// The complete game state
        /// </summary>
        private GameState Game { get; set; }

        /// <summary>
        /// Start the game!
        /// 
        /// Also acts as the reset button while the game is in progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            GameInProgress = !GameInProgress;
        }

        /// <summary>
        /// Enable/disable the "sumbit move" button depending on whether there is a move typed in or not
        /// 
        /// We only care if the field is non-empty; if a garbage move is typed in the parser will take care of it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSubmit.IsEnabled = !string.IsNullOrEmpty((sender as TextBox).Text);
        }

        /// <summary>
        /// Submit the move, make sure it's legal, and apply it to the state of the game
        /// 
        /// Will show a notification if there was an invalid move typed in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            lblError.Visibility = Visibility.Collapsed;

            try
            {
                TakMove move;
                
                if(Game.TurnNumber == 0)
                    move = new TakMove(CurrentPlayer == TakPiece.PieceColor.Black ? TakPiece.PieceColor.White : TakPiece.PieceColor.Black, ActiveInput.Text);
                else
                    move = new TakMove(CurrentPlayer, ActiveInput.Text);

                string err;
                if(move.CheckAndApply(Game, out err))
                {
                    ActiveOutput.Text += string.Format("{0:00}. {1}\n", Game.TurnNumber, move);

                    string state = string.Format("{0:00}. {1}\n{2}\n", Game.TurnNumber, move, Board);
                    imgBoard.Source = Bitmap2Source(Board.Draw());
                    
                    if (!Game.GameOver)
                    {
                        if (CurrentPlayer == TakPiece.PieceColor.White)
                        {
                            CurrentPlayer = TakPiece.PieceColor.Black;
                        }
                        else
                        {
                            CurrentPlayer = TakPiece.PieceColor.White;
                            Game.TurnNumber++;
                        }
                    }
                    else
                    {
                        btnSubmit.IsEnabled = false;
                        ActiveInput.IsEnabled = false;

                        TakPiece.PieceColor? winner = Game.Winner(CurrentPlayer);

                        if(winner != null)
                            lblError.Content = string.Format("{0} Player Wins!",winner);
                        else
                            lblError.Content = "Draw!";
                        lblError.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    lblError.Content = "Illegal move: " + err;
                    lblError.Visibility = Visibility.Visible;
                }
            }
            catch(Exception err)
            {
                lblError.Content = "Error: " + err.Message;
                lblError.Visibility = Visibility.Visible;
            }
        }

        private Dictionary<TakPiece.PieceColor, bool> IsAI = new Dictionary<TakPiece.PieceColor, bool>();
        private Dictionary<TakPiece.PieceColor, PlayerAI> AIs = new Dictionary<TakPiece.PieceColor, PlayerAI>();

        private void chkWhiteIsAI_Checked(object sender, RoutedEventArgs e)
        {
            IsAI[TakPiece.PieceColor.White] = true;
        }

        private void chkWhiteIsAI_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAI[TakPiece.PieceColor.White] = false;
        }

        private void chkBlackIsAI_Checked(object sender, RoutedEventArgs e)
        {
            IsAI[TakPiece.PieceColor.Black] = true;
        }

        private void chkBlackIsAI_Unchecked(object sender, RoutedEventArgs e)
        {
            IsAI[TakPiece.PieceColor.Black] = false;
        }

        public static BitmapSource Bitmap2Source(System.Drawing.Bitmap src)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                BitmapImage dst = new BitmapImage();
                dst.BeginInit();
                dst.StreamSource = ms;
                dst.CacheOption = BitmapCacheOption.OnLoad;
                dst.EndInit();
                dst.Freeze();
                return dst;
            }
        }

        public static StackPanel DebugArea { get; private set; }
    }
}
