using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;


namespace SimpleCopyPasteTool.View
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        public List<star> stars = new List<star>();
        private DispatcherTimer movementTimer;
        private SolidColorBrush blackBrush;

        public AboutView()
        {
            InitializeComponent();
            //tbmarquee.Text = Constants.ASCII_ART_STRING;

            this.Loaded += Window_Loaded;

            movementTimer = new DispatcherTimer(){IsEnabled = false};
            movementTimer.Tick += new EventHandler(UpdateStars);
            movementTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            movementTimer.Stop();

            blackBrush = new SolidColorBrush();
            //blackBrush.Color = Colors.White;
            blackBrush.Color = Colors.Black;
        }

        private void Window_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            //            DoubleAnimation doubleAnimation = new DoubleAnimation();
            //            doubleAnimation.From = -tbmarquee.ActualHeight;
            //            doubleAnimation.To = canMain.ActualHeight;
            //            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //            doubleAnimation.Duration = new Duration(TimeSpan.Parse("0:0:15"));
            //            tbmarquee.BeginAnimation(Canvas.TopProperty, doubleAnimation);
            int centerWidth = (int) Math.Round(canMain.ActualWidth / 4);
            int centerHeight = (int) Math.Round(canMain.ActualHeight / 4);
            for (int i = 0; i < 400; ++i)
            {
                stars.Add(new star((int)Math.Round(canMain.ActualWidth), (int)Math.Round(canMain.ActualHeight)));
            }

            Random myRandom = new Random();
            var centerOfScreen = new Point(centerWidth, centerHeight);
            foreach (star S in stars)
            {
                S.placeCenter(centerOfScreen,
                    new Point(centerOfScreen.X + (myRandom.Next(400) * ((myRandom.Next(2) == 1) ? 1 : -1)),
                        centerOfScreen.Y + (myRandom.Next(400) * ((myRandom.Next(2) == 1) ? 1 : -1))));
                S.t = myRandom.Next(50);
            }

            //To avoid the "Starburst" starting bug.
            for (int i = 0; i < 10; i++)
            {
                foreach (star S in stars)
                {
                    S.move();
                }
            }

            UpdateStars(null, null);

            movementTimer.Start();
        }

        private void UpdateStars(object sender, EventArgs eventArgs)
        {
            canMain.Children.Clear();



            foreach (star S in stars)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = blackBrush;
                ellipse.Width = S.boundingBox.Width;
                ellipse.Height = S.boundingBox.Height;
                //                ellipse.Width = 100;
                //                ellipse.Height = 100;
                //
                //                double left = desiredCenterX - (100 / 2);
                //                double top = desiredCenterY - (100 / 2);

                ellipse.Margin = new Thickness(S.boundingBox.Left, S.boundingBox.Top, S.boundingBox.Right, S.boundingBox.Bottom);
                //ellipse.Margin = new Thickness(left, top, 0, 0);

                canMain.Children.Add(ellipse);

                S.move();
                //Graphics0.FillEllipse(paintbrush, S.boundingBox);
            }
        }

        private void AboutWebsite_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.kleinprodesign.nl");
        }
    }

    public class star
    {
        private int width;
        private int height;

        public star(int _width, int _height)
        {
            this.width = _width;
            this.height = _height;
            Center = new Point();
            scale = 1;
            T = 1;
            MAX_SCALE = height / 36;
            //MAX_SCALE = Screen.PrimaryScreen.Bounds.Height / 36;
        }

        public star(Point centerOfScreen, Point startingPoint)
        {
            setInitials(centerOfScreen, startingPoint);
            T = 1;
            MAX_SCALE = height / 36;
            //MAX_SCALE = Screen.PrimaryScreen.Bounds.Height / 36;
        }

        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle(Center, new Size(Scale, Scale)){};
            }
        }

        public void placeCenter(Point centerOfScreen, Point startingPoint)
        {
            setInitials(centerOfScreen, startingPoint);
        }

        public Point center
        {
            get
            {
                return Center;
            }
        }

        public int scale
        {
            set
            {
                Scale = (value > 0 ? value : 1);
            }
        }

        public int t
        {
            set
            {
                T = value;
                scale = scaler();
            }
        }

        private int scaler()
        {
            return (int)(System.Math.Pow(++T, 2) / 100 + 2);
        }

        private void reset()
        {
            if (Scale >= MAX_SCALE)
            {
                Center = initialCenter;
                Scale = 1;
                T = 1;
            }
        }

        public void move()
        {
            reset();
            scale = scaler();
            Center.X += (int)(T * cosTheta);
            Center.Y += (int)(T * sinTheta);
        }

        private void setInitials(Point centerOfScreen, Point startingPoint)
        {
            Center = initialCenter = startingPoint;

            if (startingPoint.X == centerOfScreen.X)
            {
                if (startingPoint.Y == centerOfScreen.Y)
                {
                    Random myRandom = new Random();
                    //setInitials(centerOfScreen, new Point(centerOfScreen.X + (myRandom.Next(Screen.PrimaryScreen.Bounds.Width / 2) * ((myRandom.Next(2) == 1) ? 1 : -1)), centerOfScreen.Y + (myRandom.Next(Screen.PrimaryScreen.Bounds.Height / 2) * ((myRandom.Next(2) == 1) ? 1 : -1))));
                    setInitials(centerOfScreen, new Point(centerOfScreen.X + (myRandom.Next(width / 2) * ((myRandom.Next(2) == 1) ? 1 : -1)), centerOfScreen.Y + (myRandom.Next(height / 2) * ((myRandom.Next(2) == 1) ? 1 : -1))));
                }

                else if (startingPoint.Y > centerOfScreen.Y)
                {
                    theta = Math.PI / 2;
                }

                else
                {
                    theta = 3 * Math.PI / 2;
                }
            }

            else if (startingPoint.Y == centerOfScreen.Y)
            {
                if (startingPoint.X > centerOfScreen.X)
                {
                    theta = 0;
                }

                else
                {
                    theta = Math.PI;
                }
            }

            else
            {
                theta = ((startingPoint.X - centerOfScreen.X < 0) ? Math.PI : 0) + System.Math.Atan((double)((double)((double)startingPoint.Y - (double)centerOfScreen.Y) / (double)((double)startingPoint.X - (double)centerOfScreen.X)));
            }

            sinTheta = Math.Sin(theta);
            cosTheta = Math.Cos(theta);
        }

        #region Private Data
        private int Scale;
        private double theta;
        private double cosTheta;
        private double sinTheta;
        private Point Center;
        private Point initialCenter;
        private int T;
        private int MAX_SCALE;
        #endregion
    }
}
