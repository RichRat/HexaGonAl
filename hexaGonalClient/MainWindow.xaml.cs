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
        private readonly Animator anim;
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
            game.Exit += (_, _) => btnReset_Click(null, null);
            KeyDown += game.OnKeyDown;
            game.GameFinished += (_, p) =>
            {
                gss.Stats.UpdateStats(p).SaveStats();
                gss.UpdateStatFields();
            };
            game.StartGame(p);
            spScore.InitScoreBord(game);
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
                spScore.Reset();
            };

            anim.RegisterAnimation(250, (_, x) => gss.Opacity = x, AnimationStyle.EaseIn);
        }
    }
}
