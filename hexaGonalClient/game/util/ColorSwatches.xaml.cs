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
using System.Reflection;

namespace hexaGonalClient.game.util
{
    /// <summary>
    /// Interaction logic for ColorSwatches.xaml
    /// </summary>
    public partial class ColorSwatches : UserControl
    {
        Color[] cols =
        {
            Colors.White,
            Colors.LightGray,
            Colors.Red,
            Colors.Orange,
            Colors.Yellow,
            Colors.GreenYellow,
            Colors.Lime,
            Colors.SeaGreen,
            Colors.Aqua,
            Colors.LightBlue,
            Colors.Fuchsia,
            Colors.Magenta,
            Colors.BlueViolet
        };

        public event EventHandler<Color> ColorSelected;
        public event EventHandler Aborted;

        //TODO event for returning a color

        public ColorSwatches()
        {
            InitializeComponent();
            

            foreach (Color c in cols)
            {
                Rectangle rect = new()
                {
                    Fill = new SolidColorBrush(c),
                    Margin = new Thickness(5),
                    Stroke = new SolidColorBrush(Util.ModColBrightness(c, -0.4)),
                    StrokeThickness = 0
                };

                rect.MouseDown += Rect_MouseDown;
                rect.MouseEnter += (o, e) => rect.StrokeThickness = 3;
                rect.MouseLeave += (o, e) => rect.StrokeThickness = 0;
                ugGrid.Children.Add(rect);
            }
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color c = Colors.Transparent;
            Rectangle rect = sender as Rectangle;
            if (rect != null)
            {
                SolidColorBrush b = rect.Fill as SolidColorBrush;
                if (b != null)
                    c = b.Color;
            }

            if (ColorSelected != null && c != Colors.Transparent)
                ColorSelected.Invoke(this, c);
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            if (Aborted != null)
                Aborted.Invoke(this, null);
        }
    }
}
