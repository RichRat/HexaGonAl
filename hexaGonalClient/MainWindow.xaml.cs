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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using hexaGoNal.game;
using hexaGonalClient.game;
using hexaGonalClient.game.util;

namespace hexaGoNal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HexMatchGame game;
        private Animator anim;
        private GameSetupScreen gss;

        public MainWindow()
        {
            anim = new(this);
            InitializeComponent();
            GameSetup();
        }

        private void GameSetup()
        {
            gss = new();
            Grid.SetRow(gss, 2);
            gss.StartGame += StartGame;
            grMain.Children.Add(gss);
        }

        private void StartGame(object sender, List<Player> p)
        {
            game = new();
            Grid.SetRow(game, 2);
            game.GameLength = gss.GameLength;
            grMain.Children.Add(game);
            game.PlayerChanged += OnPlayerChanged;
            game.RoundWon += OnRoundWon;
            game.Exit += (_, _) => btnReset_Click(null, null);
            game.Reset += (_, _) => spScore.Children.Clear();
            KeyDown += game.OnKeyDown;
            game.StartGame(p);
        }

        private void OnRoundWon(object sender, Player p)
        {
            spScore.Children.Add(new Ellipse
            {
                Height = 22,
                Width = 22,
                Fill = p.Brush,
                Margin = new Thickness { Right = 12 }
            });
        }

        private void OnPlayerChanged(object sender, Player pl)
        {
            RectSpacer.Fill = new SolidColorBrush(pl.Color);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (game == null)
                return;

            gss.ResetScore();

            var a = anim.RegisterAnimation(250, (_, x) => game.Opacity = 1 - x, AnimationStyle.EaseIn);
            a.AnimationFinished = () =>
            {
                grMain.Children.Remove(game);
                game = null;
                RectSpacer.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#444");
                spScore.Children.Clear();
            };

            anim.RegisterAnimation(250, (_, x) => gss.Opacity = x, AnimationStyle.EaseIn);
        }
    }
}
