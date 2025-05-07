using hexaGoNal.game;
using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hexaGonalClient.game
{
    /// <summary>
    /// Interaction logic for GameSetupScreen.xaml
    /// </summary>
    public partial class GameSetupScreen : UserControl
    {
        private readonly Player p1 = new(Colors.Orange, "Player 1");
        private readonly Player p2 = new(Colors.Cyan, "Player 2");
        private bool inpP1init = true;
        private bool inpP2init = true;
        private readonly Animator anim;
        public int GameLength { get; set; } = -1;
        public StatTracker Stats { get; set; } = null;

        public event EventHandler<List<Player>> StartGame;


        public GameSetupScreen()
        {
            InitializeComponent();
            anim = new();
            anim.RegisterAnimation(300, (_, x) => Opacity = x, AnimationStyle.EaseIn);

            p1.Color = Util.ColFromStr(Properties.Settings.Default.Player1Color);
            p1.Name = Properties.Settings.Default.Player1Name;
            inpPlayer1.Text = p1.Name;
            SetCol(p1, colPlayer1, inpPlayer1);

            p2.Color = Util.ColFromStr(Properties.Settings.Default.Player2Color);
            p2.Name = Properties.Settings.Default.Player2Name;
            inpPlayer2.Text = p2.Name;
            SetCol(p2, colPlayer2, inpPlayer2);

            p2.Difficulty = (Difficulty)Properties.Settings.Default.difficulty;
            if (p2.IsBot)
                inpPlayer2.IsEnabled = false;

            inpDifficulty.ReadEnumContent(Difficulty.Advanced);
            inpDifficulty.SeletedItem = (int)p2.Difficulty;
            inpDifficulty.SelectedChanged += inpDifficulty_SelectedChanged;

            //TODO remove line when ai tree feature is working
            inpDifficulty.RemoveItem((int)Difficulty.Master);

            for (int i = 0; i < Properties.Settings.Default.maxGameLen; i++)
                if (i == 0 || i % 2 == 1)
                    inpGameLength.AddItem(i, i > 0 ? i.ToString() : "inf");
            
            inpGameLength.SeletedItem = GameLength = Properties.Settings.Default.gameLen;

            Stats = new()
            {
                Diff = p2.Difficulty,
                GameLength = GameLength
            };

            UpdateStatFields();
        }

        private void colPlayer1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectPlayerColor(p1, colPlayer1, inpPlayer1);
        }

        private void colPlayer2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectPlayerColor(p2, colPlayer2, inpPlayer2);
        }

        private void SelectPlayerColor(Player p, Rectangle rect, TextBox inp)
        {
            ColorSwatches cs = new()
            {
                Width = ActualWidth * 2 / 3,
                Height = ActualHeight - 40
            };

            Canvas.SetLeft(cs, ActualWidth / 6);
            Canvas.SetTop(cs, ActualHeight / 10);

            Rectangle bg = new()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Opacity = 0.6,
                Height = this.ActualHeight * 1.5,
                Width = this.ActualWidth * 1.4
            };

            Binding b = new Binding
            {
                Source = cs,
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(bg, Rectangle.OpacityProperty, b);

            canvOverlay.Children.Add(bg);
            canvOverlay.Children.Add(cs);
            cs.Aborted += (o, e) => canvOverlay.Children.Clear();
            cs.ColorSelected += (e, c) =>
            {
                p.Color = c;
                SetCol(p, rect, inp);
                canvOverlay.Children.Clear();
            };
        }

        private static void SetCol(Player p, Rectangle rect, TextBox inp)
        {
            rect.Fill = p.Brush;
            inp.BorderBrush = p.Brush;
        }

        private void inpPlayer1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (inpP1init)
            {
                inpPlayer1.Text = "";
                inpPlayer1.Foreground = new SolidColorBrush(Colors.White);
                inpP1init = false;
            }
        }

        private void inpPlayer2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (inpP2init)
            {
                inpPlayer2.Text = "";
                inpPlayer2.Foreground = new SolidColorBrush(Colors.White);
                inpP2init = false;
            }
        }

        private void inpPlayer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inpPlayer1.Text.Length > 0)
                p1.Name = inpPlayer1.Text;
        }

        private void inpPlayer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inpPlayer2.Text.Length > 0)
                p2.Name = inpPlayer2.Text;
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (StartGame == null)
                return;

            Properties.Settings.Default.Player1Name = p1.Name;
            Properties.Settings.Default.Player1Color = Util.StrFromColor(p1.Color);
            Properties.Settings.Default.Player2Name = p2.Name;
            Properties.Settings.Default.Player2Color = Util.StrFromColor(p2.Color);
            Properties.Settings.Default.gameLen = GameLength = inpGameLength.SeletedItem;
            Properties.Settings.Default.difficulty = (int)p2.Difficulty;
            Properties.Settings.Default.Save();

            anim.RegisterAnimation(300, (_, x) => Opacity = (1 - x), AnimationStyle.EaseIn)
                .AnimationFinished = () => StartGame?.Invoke(this, [p1, p2]);
        }

        private void inpDifficulty_SelectedChanged(object sender, object e)
        {
            if (e is Difficulty diff)
            {
                p2.Difficulty = diff;
                if (diff > Difficulty.HotSeat)
                {
                    inpPlayer2.Text = "Bot " + diff;
                    inpPlayer2.IsEnabled = false;
                }
                else
                {
                    inpPlayer2.Text = "Player 2";
                    inpPlayer2.IsEnabled = true;
                }

                if (Stats.Diff != diff)
                {
                    Stats.Diff = diff;
                    UpdateStatFields();
                }
            }
        }

        private void inpGameLength_SelectedChanged(object sender, object e)
        {
            int len = inpGameLength.SeletedItem;
            if (Stats.GameLength != len)
            {
                Stats.GameLength = len;
                UpdateStatFields();
            }
        }

        public void UpdateStatFields()
        {
            if (Stats == null)
                return;

            if (Stats.Diff == Difficulty.HotSeat)
            {
                TxtWinsDiff.Text = "";
                TxtWinsDiffLen.Text = "";
            }
            else
            {
                TxtWinsDiff.Text = Stats.getGlobalStat() + " Wins";
                if (Stats.GameLength > 0)
                    TxtWinsDiffLen.Text = Stats.getCurrentStat() + " Wins";
                else
                    TxtWinsDiffLen.Text = "";
            }
        }

        public void ResetScore()
        {
            p1.Score = 0;
            p2.Score = 0;
        }
    }
}
