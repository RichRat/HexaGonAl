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
        private SolidColorBrush fill;
        private Player player;

        private Action undoState;

        //TODO add state LastPlaced to show the next player what happened
        public enum DotState
        {
            DEFALUT = 0,
            PREVIEW = 1,
            WIN = 2
        }

        private DotState state = DotState.DEFALUT;
        private DotState prevState;

        public Dot(Player p, double diam)
        {
            player = p;
            fill = new SolidColorBrush();
            fill.Color = p.Color;
            shape.Fill = fill;
            shape.Height = diam;
            shape.Width = diam;

            prevState = state;
        }



        private void OnStateChanged()
        {
            undoState?.Invoke();

            switch (state)
            {
                case DotState.DEFALUT:
                    break;

                case DotState.PREVIEW:
                    shape.Stroke = shape.Fill.Clone();
                    shape.Fill = new SolidColorBrush(Colors.Transparent);
                    shape.StrokeThickness = 4;
                    undoState = () =>
                    {
                        shape.Fill = new SolidColorBrush(player.Color);
                        shape.StrokeThickness = 0;
                    };
                    break;

                case DotState.WIN:
                    shape.Stroke = new SolidColorBrush(Colors.White);
                    shape.StrokeThickness = 2;
                    shape.Effect = new DropShadowEffect {
                        ShadowDepth = 0,
                        Color = Util.ChangeColorBrightness(player.Color, 0.2),
                        Opacity = 1,
                        BlurRadius = 64
                    };
                    undoState = () =>
                    {
                        shape.StrokeThickness = 0;
                        shape.Effect = null;
                    };
                    break;

                default:
                    break;
            }
        }

        //TODO move to util class
  

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
    }
}
