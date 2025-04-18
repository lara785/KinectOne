using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectBallControl
{
    public partial class MainWindow : Window
    {
        private KinectSensor kinectSensor;
        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;
        private Ellipse ball;
        private double ballSize = 50;

        public MainWindow()
        {
            InitializeComponent();
            this.kinectSensor = KinectSensor.GetDefault();
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Open();
                this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
                this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            }

            // Create a ball (Ellipse) on the screen
            ball = new Ellipse
            {
                Width = ballSize,
                Height = ballSize,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(ball, 300);
            Canvas.SetTop(ball, 200);
            this.canvas.Children.Add(ball);
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    foreach (var body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            TrackUser(body);
                        }
                    }
                }
            }
        }

        private void TrackUser(Body body)
        {
            var leftHand = body.Joints[JointType.HandLeft].Position;
            var rightHand = body.Joints[JointType.HandRight].Position;
            var leftFoot = body.Joints[JointType.FootLeft].Position;
            var rightFoot = body.Joints[JointType.FootRight].Position;

            // Detect arms opening (hands farther apart)
            double armDistance = Math.Sqrt(Math.Pow(leftHand.X - rightHand.X, 2) + Math.Pow(leftHand.Y - rightHand.Y, 2) + Math.Pow(leftHand.Z - rightHand.Z, 2));

            if (armDistance > 0.5) // Threshold for arms opening
            {
                ballSize += 1; // Increase ball size
                ball.Width = ballSize;
                ball.Height = ballSize;
            }

            // Detect kicking (foot movement)
            double footDistance = Math.Sqrt(Math.Pow(leftFoot.Y - rightFoot.Y, 2));

            if (footDistance > 0.2) // Threshold for kicking
            {
                // Ball flies away (move the ball to a random position)
                Random rand = new Random();
                double newX = rand.Next(100, 500);
                double newY = rand.Next(100, 500);
                Canvas.SetLeft(ball, newX);
                Canvas.SetTop(ball, newY);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }

            base.OnClosed(e);
        }
    }
}
