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
using System.Windows.Media.Effects;
using static hexaGonalClient.game.Animator;

namespace hexaGonalClient.game.util
{
    /// <summary>
    /// Interaction logic for ColorSwatches.xaml
    /// </summary>
    public partial class ColorSwatches : UserControl
    {
        private static readonly Color[] cols =
        {
            Colors.White,
            Colors.WhiteSmoke,
            Colors.LightGray,
            Colors.IndianRed,
            Colors.PaleVioletRed,
            Colors.Red,
            Colors.OrangeRed,
            Colors.Orange,
            Colors.Beige,
            Colors.Yellow,
            Colors.GreenYellow,
            Colors.Chartreuse,
            Colors.Lime,
            Colors.SeaGreen,
            Colors.Aqua,
            Colors.Cyan,
            Colors.LightBlue,
            Colors.MediumBlue,
            Colors.Fuchsia,
            Colors.Magenta,
            Colors.BlueViolet,
            Colors.MediumVioletRed
        };

        public event EventHandler<Color> ColorSelected;
        public event EventHandler Aborted;

        private readonly Animator anim;

        public ColorSwatches()
        {
            InitializeComponent();

            //this.Effect = new DropShadowEffect
            //{
            //    ShadowDepth = 0,
            //    Color = Colors.Black,
            //    BlurRadius = 15
            //};

            foreach (Color c in cols)
            {
                Rectangle rect = new()
                {
                    Fill = new SolidColorBrush(c),
                    Margin = new Thickness(5),
                    Stroke = new SolidColorBrush(Util.ModColBrightness(c, -0.4)),
                    StrokeThickness = 0,
                    MinWidth = 32,
                    MinHeight = 32
                };

                rect.MouseDown += Rect_MouseDown;
                rect.MouseEnter += (o, e) => rect.StrokeThickness = 3;
                rect.MouseLeave += (o, e) => rect.StrokeThickness = 0;
                ugGrid.Children.Add(rect);
            }

            anim = new();
            Opacity = 0;
            anim.RegisterAnimation(700, (o, x) => Opacity = x, AnimationStyle.EaseOut);
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
                DelayResponse(() => ColorSelected.Invoke(this, c));
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            if (Aborted == null)
                return;

            DelayResponse(() => Aborted.Invoke(this, null));
        }

        private void DelayResponse(Action a)
        {
            Animation an = anim.RegisterAnimation(350, (_, x) => Opacity = (1 - x), AnimationStyle.EaseOut);
            an.AnimationFinished = a;
        }
    }
}
