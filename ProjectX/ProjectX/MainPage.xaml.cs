using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Kinect;


using Windows.UI.Xaml.Media.Imaging;


using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;

namespace ProjectX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        KinectSensor sensor;
        InfraredFrameReader irReader;
        //BodyFrameReader bodyReader;

        ushort[] irData;
        byte[] irDataConverted;
        WriteableBitmap irBitmap;

        Body[] bodies;
        MultiSourceFrameReader msfr;

        GestureRecognitionEngine gestureEngine;


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {
                irReader = sensor.InfraredFrameSource.OpenReader();
                FrameDescription fd = sensor.InfraredFrameSource.FrameDescription;
                irData = new ushort[fd.LengthInPixels];
                irDataConverted = new byte[fd.LengthInPixels * 4];
                irBitmap = new WriteableBitmap(fd.Width, fd.Height);
                image.Source = irBitmap;

                bodies = new Body[6];
                msfr = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);// | FrameSourceTypes.Infrared);
                msfr.MultiSourceFrameArrived += Msfr_MultiSourceFrameArrived;


                gestureEngine = new GestureRecognitionEngine();
                gestureEngine.GestureRecognized += new EventHandler<GestureEventArgs>(swipeGestureRecognized);

                sensor.Open();
                //irReader.FrameArrived += irReader_FrameArrived;
            }
        }

        private void swipeGestureRecognized(object sender, GestureEventArgs e)
        {
            if (e.GestureType.Equals(GestureType.SwipeLeftGestureWithRightHand))
            {
                swipeLeft();
                return;
            }
            if (e.GestureType.Equals(GestureType.SwipeRightGestureWithRightHand))
            {
                swipeRight();
                return;
            }
        }

        private void swipeRight()
        {
            print("Swipe Right");
        }

        private void swipeLeft()
        {
            print("Swipe Left");
        }

        private void Msfr_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            using (MultiSourceFrame msf = args.FrameReference.AcquireFrame())
            {
                if (msf != null)
                {
                    using (BodyFrame bodyFrame = msf.BodyFrameReference.AcquireFrame())
                    {
                        bodyFrame.GetAndRefreshBodyData(bodies);
                        bodyCanvas.Children.Clear();

                        bodyFrame.GetAndRefreshBodyData(bodies);
                        bodyCanvas.Children.Clear();

                        //Body body = bodies[0];
                        foreach (Body body in bodies)
                        {
                            if (body.IsTracked)
                            {
                                /*Joint headJoint = body.Joints[JointType.Head];
                                Joint rightHand = body.Joints[JointType.HandRight];
                                if (rightHand.TrackingState == TrackingState.Tracked)
                                {
                                    DepthSpacePoint dsp = sensor.CoordinateMapper.MapCameraPointToDepthSpace(rightHand.Position);
                                    Ellipse headCircle = new Ellipse() { Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) };
                                    bodyCanvas.Children.Add(headCircle);
                                    Canvas.SetLeft(headCircle, dsp.X - 25);
                                    Canvas.SetTop(headCircle, dsp.Y - 25);
                                }*/

                                gestureEngine.Body = body;
                                gestureEngine.StartRecognize();
                            }
                        }

                        /*using (InfraredFrame irFrame = msf.InfraredFrameReference.AcquireFrame())
                        {
                            if (irFrame != null && bodyFrame != null)
                            {
                                irFrame.CopyFrameDataToArray(irData);

                                for (int i = 0; i < irData.Length; i++)
                                {
                                    byte intensity = (byte)(irData[i] >> 8);
                                    irDataConverted[i * 4] = intensity;
                                    irDataConverted[i * 4 + 1] = intensity;
                                    irDataConverted[i * 4 + 2] = intensity;
                                    irDataConverted[i * 4 + 3] = intensity;
                                }
                                irDataConverted.CopyTo(irBitmap.PixelBuffer);
                                irBitmap.Invalidate();

                                bodyFrame.GetAndRefreshBodyData(bodies);
                                bodyCanvas.Children.Clear();

                                //Body body = bodies[0];
                                foreach (Body body in bodies)
                                {
                                    if (body.IsTracked)
                                    {
                                        Joint headJoint = body.Joints[JointType.Head];
                                        Joint rightHand = body.Joints[JointType.HandRight];
                                        if (rightHand.TrackingState == TrackingState.Tracked)
                                        {
                                            DepthSpacePoint dsp = sensor.CoordinateMapper.MapCameraPointToDepthSpace(rightHand.Position);
                                            Ellipse headCircle = new Ellipse() { Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)) };
                                            bodyCanvas.Children.Add(headCircle);
                                            Canvas.SetLeft(headCircle, dsp.X - 25);
                                            Canvas.SetTop(headCircle, dsp.Y - 25);
                                        }

                                        gestureEngine.Body = body;
                                        gestureEngine.StartRecognize();
                                    }
                                }
                            }
                        }*/
                    }
                }
            }
        }

        /// <summary>
        /// Writes the debug statement to the console
        /// </summary>
        /// <param name="debugStatement"></param>
        private void print(string debugStatement)
        {
#if DEBUG
            Debug.WriteLine(debugStatement);
#endif
        }

    }
}
