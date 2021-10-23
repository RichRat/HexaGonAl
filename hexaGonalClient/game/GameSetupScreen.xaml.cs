using hexaGoNal;
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
        private Player p1 = new(Colors.Orange, "Player 1");
        private Player p2 = new(Colors.Cyan, "Player 2");
        private bool inpP1init = true;
        private bool inpP2init = true;
        private Animator anim;

        public event EventHandler<List<Player>> StartGame;

        public GameSetupScreen()
        {
            InitializeComponent();
            anim = new(this);
            anim.RegisterAnimation(Animator.AnimationStyle.EaseIn, 300, (_, x) => Opacity = x);
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
                rect.Fill = p.Brush;
                inp.BorderBrush = p.Brush;
                canvOverlay.Children.Remove(cs);
            };
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

            Animation an = anim.RegisterAnimation(Animator.AnimationStyle.EaseIn, 300, (_, x) => Opacity = (1 - x));
            an.AnimationFinished = () => StartGame.Invoke(this, new() { p1, p2 });
        }
    }
}
