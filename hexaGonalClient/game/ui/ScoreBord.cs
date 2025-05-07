using hexaGoNal.game;
using hexaGonalClient.game.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace hexaGonalClient.game.ui
{
    class ScoreBord : StackPanel
    {
        private int gameLen = 0;
        private int scoreIndex = 0;
        private readonly Animator anim;

        public ScoreBord()
        {
            this.anim = new();
        }

        public void InitScoreBord(HexMatchGame game)
        {
            this.gameLen = game.GameLength;
            game.RoundWon += OnRoundWon;
            game.Reset += OnStartGame;
            OnStartGame();
        }

        private void OnStartGame(object sender = null, EventArgs e = null)
        {
            Children.Clear();
            scoreIndex = 0;
            if (gameLen > 0)
                anim.RegisterAnimation(gameLen * 180, (o, x) => {
                    while (Children.Count < x * gameLen)
                    {
                        Children.Add(new Ellipse
                        {
                            Height = 22,
                            Width = 22,
                            Fill = new SolidColorBrush(Colors.Transparent),
                            Stroke = new SolidColorBrush(Colors.White),
                            Opacity = 0.75,
                            StrokeThickness = 3,
                            Margin = new Thickness { Right = 12 }
                        });
                    }

                }, AnimationStyle.Linear);
        }

        private void OnRoundWon(object sender, Player p)
        {
            if (gameLen > 0)
                Children.RemoveAt(scoreIndex);

            Children.Insert(scoreIndex, new Ellipse
            {
                Height = 22,
                Width = 22,
                Fill = p.Brush,
                Margin = new Thickness { Right = 12 }
            });

            scoreIndex++;
        }

        internal void Reset()
        {
            anim.RegisterAnimation(250, (o, x) => Opacity = (1 - x), AnimationStyle.EaseIn)
                .AnimationFinished = () =>
                {
                    Children.Clear();
                    Opacity = 1;
                };
        }
    }
}
