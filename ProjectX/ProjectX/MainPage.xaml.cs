using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
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
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.VisualBasic;
using Windows.UI.Xaml.Media.Imaging;


using Windows.UI.Xaml.Shapes;
using Windows.UI;
using System.Diagnostics;

namespace ProjectX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        private string[] IMAGES = { "projectx_4.jpg", "projectx_7.jpg", "projectx_12.jpg", "projectx_13.jpg", "projectx_15.jpg", "projectx_17.jpg", "projectx_22.jpg", "projectx_32.jpg", "projectx_39.jpg", "projectx_43.jpg", "projectx_44.jpg", "projectx_45.jpg", "projectx_55.jpg", "projectx_61.jpg" };    // images
        private static double IMAGE_WIDTH = 128;        // Image Width
        private static double IMAGE_HEIGHT = 128;       // Image Height        
        private static double SPRINESS = 0.2;		    // Control the Spring Speed
        private static double DECAY = 0.5;			    // Control the bounce Speed
        private static double SCALE_DOWN_FACTOR = 0.025;  // Scale between images
        private static double OFFSET_FACTOR = 100;      // Distance between images
        private static double OPACITY_DOWN_FACTOR = 0.4;    // Alpha between images
        private static double SCALING;            // Maximum Scale
        private static int TIMER = 4000;//in millieconds
        private static float VIEW_FRUSTUM_Z = 1.5f;
        private static float VIEW_FRUSTUM_X = 0.5f;

        private DispatcherTimer slideshowTimer = new DispatcherTimer();

        private double _xCenter;
        private double _yCenter;

        private double _target = 0;		// Target moving position
        private double _current = 0;	// Current position
        private double _spring = 0;		// Temp used to store last moving 
        private List<Image> _images = new List<Image>();	// Store the added images

        private static int FPS = 24;                // fps of the on enter frame event
        private DispatcherTimer _timer = new DispatcherTimer(); // on enter frame simulator

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.SizeChanged += OnWindowSizeChanged;
            this.slideshowTimer.Interval = TimeSpan.FromMilliseconds(TIMER);
            this.slideshowTimer.Tick += next_slide;
            slideshowTimer.Start();

            addImages();
        }

        private void next_slide(object sender, object e)
        {
            moveIndex(1);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = e.NewSize.Width;
            double oldWindowHeight = e.PreviousSize.Height;
            double oldWindowWidth = e.PreviousSize.Width;
            MPage.Height = newWindowHeight;
            MPage.Width = newWindowWidth;

            print("Height = " + newWindowHeight);
            print("Width = " + newWindowWidth);
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
            Start();
            
            Debug.WriteLine(this.ActualHeight);
            Debug.WriteLine(this.ActualWidth);
            OFFSET_FACTOR = this.ActualWidth;
            sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {
                irReader = sensor.InfraredFrameSource.OpenReader();
                FrameDescription fd = sensor.InfraredFrameSource.FrameDescription;
                irData = new ushort[fd.LengthInPixels];
                irDataConverted = new byte[fd.LengthInPixels * 4];
                irBitmap = new WriteableBitmap(fd.Width, fd.Height);
                //image.Source = irBitmap;

                bodies = new Body[6];
                msfr = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);// | FrameSourceTypes.Infrared);
                msfr.MultiSourceFrameArrived += Msfr_MultiSourceFrameArrived;


                gestureEngine = new GestureRecognitionEngine();
                gestureEngine.GestureRecognized += new EventHandler<GestureEventArgs>(swipeGestureRecognized);

                sensor.Open();
                //irReader.FrameArrived += irReader_FrameArrived;
            }
        }

        /// <summary>
        /// Function that raises events when the gestures are recognized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void swipeGestureRecognized(object sender, GestureEventArgs e)
        {
            if (e.GestureType.Equals(GestureType.SwipeLeftGestureWithRightHand) ||
                e.GestureType.Equals(GestureType.SwipeLeftGestureWithLeftHand))
            {
                swipeLeft();
                return;
            }
            if (e.GestureType.Equals(GestureType.SwipeRightGestureWithRightHand) || 
                e.GestureType.Equals(GestureType.SwipeRightGestureWithLeftHand))
            {
                swipeRight();
                return;
            }
        }

        private void swipeRight()
        {
            print("Swipe Right");
            moveIndex(-1);
        }

        private void swipeLeft()
        {
            print("Swipe Left");
            moveIndex(1);
        }

        /// <summary>
        /// Checks if the body is in the correct view frustum
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private bool ifTrackable(Body body)
        {
            print("Body x = " + body.Joints[JointType.SpineBase].Position.X);
            print("Body y = " + body.Joints[JointType.SpineBase].Position.Y);
            print("Body z = " + body.Joints[JointType.SpineBase].Position.Z);

            return (body.Joints[JointType.SpineBase].Position.X <= VIEW_FRUSTUM_X && body.Joints[JointType.SpineBase].Position.X >= -VIEW_FRUSTUM_X) &&
                (body.Joints[JointType.SpineBase].Position.Z <= VIEW_FRUSTUM_Z+1 && body.Joints[JointType.SpineBase].Position.Z >= VIEW_FRUSTUM_Z);
        }

        private void Msfr_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            using (MultiSourceFrame msf = args.FrameReference.AcquireFrame())
            {
                if (msf != null)
                {
                    using (BodyFrame bodyFrame = msf.BodyFrameReference.AcquireFrame())
                    {
                        if (bodyFrame != null)
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

                                    if (ifTrackable(body))
                                    {
                                        print("Body is being tracked");
                                        gestureEngine.Body = body;
                                        gestureEngine.StartRecognize();
                                    }
                                    Body_Tracking_Highlight(ifTrackable(body));
                                }
                            }
                        }
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



        /////////////////////////////////////////////////////        
        // Handlers 
        /////////////////////////////////////////////////////	

        // reposition the images
        void _timer_Tick(object sender, object e)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                Image image = _images[i];
                posImage(image, i);
            }


            // compute the current position
            // added spring effect
            if (_target == _images.Count)
                _target = 0;
            _spring = (_target - _current) * SPRINESS + _spring * DECAY;
            _current += _spring;
        }

        /////////////////////////////////////////////////////        
        // Private Methods 
        /////////////////////////////////////////////////////	


        // add images to the stage
        private void addImages()
        {
            for (int i = 0; i < IMAGES.Length; i++)
            {
                // get the image resources from the xap
                string url = IMAGES[i];

                try
                {
                    Image image = new Image();
                    print(url + " exists");
                    loadImage(image, url);
                    // add and reposition the image
                    LayoutRoot.Children.Add(image);
                    posImage(image, i);
                    _images.Add(image);
                }
                catch (FileNotFoundException)
                {
                    print(url + " does not exists");
                }

            }
        }

        private async void loadImage(Image image,String url)
        {

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/"+url));
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage im = new BitmapImage();
                im.SetSource(fileStream);
                image.Source = im;
                image.Height = im.PixelHeight;
                image.Width = im.PixelWidth;
                Debug.WriteLine(image.Height);

            }
        }

        // Takes image and returns the width of the image
        private double getImageWidth(Image image)
        {
            return 800;
        }

        // Takes image and returns the height of the image
        private double getImageHeight(Image image)
        {
            return 800;
        }

        private double getImageScale(double width, double height)
        {
            return 0.4;
        }

        // move the index
        private void moveIndex(int value)
        {
            _target += value;
            _target = Math.Max(0, _target);
            _target = Math.Min(_images.Count - 1, _target);
        }

        // reposition the image
        private void posImage(Image image, int index)
        {
            double diffFactor = index - _current;


            // scale and position the image according to their index and current position
            // the one who closer to the _current has the larger scale
            ScaleTransform scaleTransform = new ScaleTransform();
            if (image.Width <= image.Height)
            {
                SCALING = this.ActualWidth / image.Width;
            }
            else
            {
                SCALING = this.ActualHeight / image.Height;
            }
            scaleTransform.ScaleX = SCALING - Math.Abs(diffFactor) * SCALE_DOWN_FACTOR;
            scaleTransform.ScaleY = SCALING - Math.Abs(diffFactor) * SCALE_DOWN_FACTOR;
            image.RenderTransform = scaleTransform;

            // reposition the image
            // double left = _xCenter - (IMAGE_WIDTH * scaleTransform.ScaleX) / 2 + diffFactor * OFFSET_FACTOR;
            double left = _xCenter - (image.Width * scaleTransform.ScaleX) / 2 + diffFactor * OFFSET_FACTOR;
            // double top = _yCenter - (IMAGE_HEIGHT * scaleTransform.ScaleY) / 2;
            double top = _yCenter - (image.Height * scaleTransform.ScaleY) / 2;
            image.Opacity = 1 - Math.Abs(diffFactor) * OPACITY_DOWN_FACTOR;

            image.SetValue(Canvas.LeftProperty, left);
            image.SetValue(Canvas.TopProperty, top);

            // order the element by the scaleX
            image.SetValue(Canvas.ZIndexProperty, (int)Math.Abs(scaleTransform.ScaleX * 100));
        }

        /////////////////////////////////////////////////////        
        // Public Methods
        /////////////////////////////////////////////////////	

        // start the timer
        public void Start()
        {
            // start the enter frame event
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / FPS);
            _timer.Tick +=  new EventHandler<object>(_timer_Tick);
            _timer.Start();

            // Save the center position
            _xCenter = this.ActualWidth / 2;
            _yCenter = this.ActualHeight / 2;
        }

        private void Body_Tracking_Highlight(bool tracked)
        {
            if (tracked)
            {
                CanvasBorder.BorderBrush = new SolidColorBrush(Colors.LightGreen);
                CanvasBorder.BorderThickness = new Thickness(6);
                if (slideshowTimer.IsEnabled)
                {
                    slideshowTimer.Stop();
                }
            } else
            {
                CanvasBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                CanvasBorder.BorderThickness = new Windows.UI.Xaml.Thickness(10);
                if (!slideshowTimer.IsEnabled)
                {
                    slideshowTimer.Start();
                }
            }
            
        }



    }

}
