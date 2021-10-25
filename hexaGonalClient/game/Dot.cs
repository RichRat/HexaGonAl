using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using hexaGonalClient.game;

namespace hexaGoNal
{
    class Dot
    {
        private readonly Ellipse shape = new Ellipse();
        private Player player;
        private Action undoState;

        public enum DotState
        {
            Default = 0,
            Preview = 1,
            Win = 2,
            LastPlaced = 4
        }

        private DotState state = DotState.Default;
        private DotState prevState;

        public Dot(Player p, double diam)
        {
            player = p;
            shape.Fill = player.Brush;
            shape.Height = diam;
            shape.Width = diam;

            prevState = state;
        }



        private void OnStateChanged()
        {
            undoState?.Invoke();

            switch (state)
            {
                case DotState.Default:
                    break;

                case DotState.Preview:
                    shape.Stroke = player.Brush;
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                    shape.StrokeThickness = 4;
                    undoState = () =>
                    {
                        shape.Fill = player.Brush;
                        shape.StrokeThickness = 0;
                    };
                    break;

                case DotState.Win:
                    shape.Stroke = new SolidColorBrush(Colors.White);
                    shape.StrokeThickness = 2;
                    shape.Effect = new DropShadowEffect {
                        ShadowDepth = 0,
                        Color = Util.ModColBrightness(player.Color, 0.5),
                        Opacity = 1,
                        BlurRadius = 64
                    };
                    undoState = () =>
                    {
                        shape.StrokeThickness = 0;
                        shape.Effect = null;
                    };
                    break;

                case DotState.LastPlaced:
                    shape.Effect = new DropShadowEffect
                    {
                        ShadowDepth = 0,
                        Color = Util.ModColBrightness(player.Color, 0.5),
                        Opacity = 1,
                        BlurRadius = 16
                    };
                    undoState = () => shape.Effect = null;
                    break;

                default:
                    break;
            }
        }

        public Shape Shape => shape;

        public DotState State
        {
            get => state;
            set {
                if (state != value)
                {
                    state = value;
                    OnStateChanged();
                    
                }
            }
        }

        public Action UndoStateAction => undoState;

        public Player Player => player;

        public bool IsWinDot()
        {
            return state == DotState.Win;
        }
    }
}
