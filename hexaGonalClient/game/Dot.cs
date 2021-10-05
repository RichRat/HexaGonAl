using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace hexaGoNal
{
    class Dot
    {
        Ellipse shape = new Ellipse();
        private SolidColorBrush fill;
        private Player player;

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

        private void OnStateChanged(DotState prev, DotState current)
        {
            //TODO implement differtent display modes: default, preview (transparent), win (add border)
        }

        public Shape Shape => shape;

        public DotState State
        {
            get => state;
            set {
                if (state != value)
                {
                    OnStateChanged(state, value);
                    state = value;
                }
            }
        }

        public Player Player => player;
    }
}
