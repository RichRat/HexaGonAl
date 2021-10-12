﻿using hexaGoNal;
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

namespace hexaGonalClient
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class WinRoundScreen : UserControl
    {
        public WinRoundScreen()
        {
            InitializeComponent();
        }

        public void EnableScreen(Player winPlayer, List<Player> pll)
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
            txtPlayer1Status.Text = pll[0].Name + " " + pll[0].Score;

            txtPlayer2Status.Foreground = pll[1].Brush;
            txtPlayer2Status.Text = pll[1].Score + " " + pll[0].Name;
        }

        public void DisableScreen()
        {
            Visibility = Visibility.Hidden;
        }
    }
}
