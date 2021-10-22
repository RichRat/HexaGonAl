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

namespace hexaGoNal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Dot> dots = new();
        HexMatchGame game;

        public MainWindow()
        {
            InitializeComponent();

            GameSetupScreen gss = new();
            Grid.SetRow(gss, 2);
            gss.StartGame += StartGame;
            grMain.Children.Add(gss);

        }

        private void StartGame(object sender, List<Player> p)
        {
            game = new();
            Grid.SetRow(game, 2);
            grMain.Children.Add(game);
            game.PlayerChanged += OnPlayerChanged;
            game.StartGame(p);
        }

        private void OnPlayerChanged(object sender, Player pl)
        {
            RectSpacer.Fill = new SolidColorBrush(pl.Color);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            game?.StartGame();
        }
    }
}
