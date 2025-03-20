using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hexaGonalClient
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class WinRoundScreen : UserControl
    {
        private Canvas parent;
        private double zoom = 1;
        private bool btnEnabled = false;

        public enum Response
        {
            Restart = 0,
            Settings = 1
        }

        public event EventHandler<Response> ButtonClick;

        public WinRoundScreen(Canvas parent)
        {
            InitializeComponent();
            parent.SizeChanged += WinRoundScreen_SizeChanged;
            this.parent = parent;
        }

        public void InnitPos()
        {
            Canvas.SetLeft(this, (parent.ActualWidth - ActualWidth * zoom) / 2);
            Canvas.SetTop(this, parent.ActualHeight / 3);
        }

        public void SetZoom(double zoom)
        {
            this.zoom = zoom;
            LayoutTransform = new ScaleTransform(zoom, zoom);
            UpdateLayout();
        }

        public double GetScrollHeight()
        {
            return ActualHeight * zoom + parent.ActualHeight / 3;
        }

        void WinRoundScreen_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                Canvas.SetLeft(this, (e.NewSize.Width - ActualWidth * zoom) / 2);
            }
        }

        public void EnableScreen(Player winPlayer, List<Player> pll, bool isEndGame = false)
        {
            if (pll.Count < 2)
                return;

            Visibility = Visibility.Visible;
            foreach (UIElement elem in stackMain.Children)
            {
                switch (elem)
                {
                    case TextBlock text: text.Foreground = winPlayer.Brush; break;
                    case Rectangle rect: rect.Fill = winPlayer.Brush; break;
                    default:
                        break;
                }
            }

            txtWinner.Text = winPlayer.Name + " Round Win";

            txtDivider.Foreground = winPlayer.Brush;

            txtPlayer1Status.Foreground = pll[0].Brush;
            txtPlayer1Status.Text = pll[0].Score.ToString();

            txtPlayer2Status.Foreground = pll[1].Brush;
            txtPlayer2Status.Text = pll[1].Score.ToString();

            if (isEndGame)
            {
                btnEnabled = true;
                txtWinner.Text = winPlayer.Name + " Game Win";
                textEndRound.Visibility = Visibility.Hidden;
                gridEndGame.Visibility = Visibility.Visible;
            }
        }

        public void DisableScreen()
        {
            Visibility = Visibility.Hidden;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (btnEnabled)
            {
                btnEnabled = false;
                ButtonClick?.Invoke(this, Response.Restart);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (btnEnabled)
            {
                btnEnabled = false;
                ButtonClick?.Invoke(this, Response.Settings);
            }
        }
    }
}
