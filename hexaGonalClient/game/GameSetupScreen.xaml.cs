using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private Difficulties Difficulty {  get; set; }

        public event EventHandler<(List<Player>, Difficulties)> StartGame;

        public GameSetupScreen()
        {
            InitializeComponent();
            anim = new(this);
            anim.RegisterAnimation(300, (_, x) => Opacity = x, AnimationStyle.EaseIn);

            p1.Color = Util.ColFromStr(Properties.Settings.Default.Player1Color);
            p1.Name = Properties.Settings.Default.Player1Name;
            inpPlayer1.Text = p1.Name;
            SetCol(p1, colPlayer1, inpPlayer1);
            
            p2.Color = Util.ColFromStr(Properties.Settings.Default.Player2Color);
            p2.Name = Properties.Settings.Default.Player2Name;
            inpPlayer2.Text = p2.Name;
            SetCol(p2, colPlayer2, inpPlayer2);

            Difficulty = (Difficulties)Properties.Settings.Default.difficulty;
            inpDifficulty.ItemsSource = Enum.GetValues(typeof(Difficulties));
            inpDifficulty.SelectedItem = Difficulty;
            
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
            Canvas.SetTop(cs, 20);

            canvOverlay.Children.Add(cs);
            cs.Aborted += (o, e) => canvOverlay.Children.Remove(cs);
            cs.ColorSelected += (e, c) =>
            {
                p.Color = c;
                SetCol(p, rect, inp);
                canvOverlay.Children.Remove(cs);
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
            Properties.Settings.Default.Save();

            Animation an = anim.RegisterAnimation(300, (_, x) => Opacity = (1 - x), AnimationStyle.EaseIn);
            an.AnimationFinished = 
                () => {
                    StartGame.Invoke(this, (new() { p1, p2 }, Difficulty));
                };
        }

        private void inpDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inpDifficulty.SelectedItem is Difficulties diff)
            {
                Properties.Settings.Default.difficulty = (int)diff;
                Difficulty = diff;

                if (diff > Difficulties.HotSeat)
                {
                    inpPlayer2.Text = "Bot " + diff;
                    inpPlayer2.IsEnabled = false;
                }
                else
                {
                    inpPlayer2.Text = "Player 2";
                    inpPlayer2.IsEnabled = true;
                }
            }
        }
    }
}
