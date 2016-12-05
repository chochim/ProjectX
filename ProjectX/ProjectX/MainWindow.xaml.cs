using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using System.Drawing;
using Microsoft.Kinect.Wpf.Controls;
using System.Reflection;

namespace ProjectX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private string[] IMAGES = { "projectx_4.jpg", "projectx_7.jpg", "projectx_12.jpg", "projectx_13.jpg", "projectx_15.jpg", "projectx_17.jpg", "projectx_22.jpg" };//, "projectx_32.jpg", "projectx_39.jpg" };//, "projectx_43.jpg", "projectx_44.jpg", "projectx_45.jpg", "projectx_55.jpg", "projectx_61.jpg" };    // images
        //private string[] NAMES = { "alzheimer", "bailey", "bethe", "blackwell", "bolton", "buck", "cerrache" };
        public string[] IMAGES = new string[98];
        public string[] NAMES = new string[98];
        private static double IMAGE_WIDTH = 128;        // Image Width
        private static double IMAGE_HEIGHT = 128;       // Image Height        
        private static double SPRINESS = 0.15;          // Control the Spring Speed
        private static double DECAY = 0.25;             // Control the bounce Speed
        private static double SCALE_DOWN_FACTOR = 0.025;  // Scale between images
        private static double OFFSET_FACTOR = 100;      // Distance between images
        private static double OPACITY_DOWN_FACTOR = 0.4;    // Alpha between images
        private static double SCALING;            // Maximum Scale
        private static int TIMER = 8000;//in millieconds
        private static float VIEW_FRUSTUM_Z = 1.8f;
        private static float VIEW_FRUSTUM_X = 0.25f;

        static bool isSpeechEnabled = true;

        private static int popTIMER = 4000;

        private DispatcherTimer slideshowTimer = new DispatcherTimer();
        private DispatcherTimer popupTimer = new DispatcherTimer();

        //Speech
        /// <summary>
        /// Stream for 32b-16b conversion.
        /// </summary>
        private KinectAudioStream convertStream = null;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine = null;


        /// <summary>
        /// List of all UI span elements used to select recognized text.
        /// </summary>
        private List<Span> recognitionSpans;

        private double _xCenter;
        private double _yCenter;

        private double _target = 0;     // Target moving position
        private double _current = 0;    // Current position
        private double _spring = 0;     // Temp used to store last moving 
        private List<Image> _images = new List<Image>();    // Store the added images

        private static int FPS = 30;                // fps of the on enter frame event
        private DispatcherTimer _timer = new DispatcherTimer(); // on enter frame simulator



        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += OnWindowSizeChanged;
            this.slideshowTimer.Interval = TimeSpan.FromMilliseconds(TIMER);
            this.slideshowTimer.Tick += next_slide;
            slideshowTimer.Start();
            this.popupTimer.Interval = TimeSpan.FromMilliseconds(popTIMER);
            popupTimer.Start();

            addImages();
        }

        /// <summary>
        /// 
        /// Takes a index and returns the time interval for that particular file
        /// "projectx_7.jpg" -> returns 7
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int getTimeInterval(int index)
        {
            string fileName = IMAGES[index];
            string[] words = fileName.Split('_');
            if (words.Length == 1)
            {
                return TIMER;
            }

            string delaystr = words[1];
            string[] parts = delaystr.Split('.');
            if (parts.Length == 1)
            {
                return TIMER;
            }
            int delay = TIMER;
            if (Int32.TryParse(parts[0], out delay))
            {
                return delay;
            }
            return delay;
        }

        private static RecognizerInfo TryGetKinectRecognizer()
        {
            IEnumerable<RecognizerInfo> recognizers;

            // This is required to catch the case when an expected recognizer is not installed.
            // By default - the x86 Speech Runtime is always expected. 
            try
            {
                recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            }
            catch (Exception)
            {
                return null;
            }

            foreach (RecognizerInfo recognizer in recognizers)
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) &&
                    "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void next_slide(object sender, object e)
        {
            moveIndex(1);
        }

        private void pop_out(object sender, object e)
        {

            Popup.Visibility = Visibility.Collapsed;
            popupTimer.Stop();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWindowHeight = e.NewSize.Height;
            double newWindowWidth = e.NewSize.Width;
            double oldWindowHeight = e.PreviousSize.Height;
            double oldWindowWidth = e.PreviousSize.Width;
            MWindow.Height = newWindowHeight;
            MWindow.Width = newWindowWidth;

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


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Start();

            Debug.WriteLine(this.ActualHeight);
            Debug.WriteLine(this.ActualWidth);
            OFFSET_FACTOR = this.ActualWidth;
            sensor = KinectSensor.GetDefault();
            if (sensor != null)
            {

                bodies = new Body[6];
                msfr = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
                msfr.MultiSourceFrameArrived += FrameArrived;
                gestureEngine = new GestureRecognitionEngine();
                gestureEngine.GestureRecognized += new EventHandler<GestureEventArgs>(swipeGestureRecognized);

                sensor.Open();

                // grab the audio stream
                IReadOnlyList<AudioBeam> audioBeamList = sensor.AudioSource.AudioBeams;
                Stream audioStream = (Stream)audioBeamList[0].OpenInputStream();

                // create the convert stream
                convertStream = new KinectAudioStream(audioStream);

                setupSpeechRecognition();

            }
        }

        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame msf = e.FrameReference.AcquireFrame();

            if (msf != null)
            {
                using (BodyFrame bodyFrame = msf.BodyFrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        bodyFrame.GetAndRefreshBodyData(bodies);
                        bodyCanvas.Children.Clear();
                        bool sweetSpot = false;
                        foreach (Body body in bodies)
                        {
                            if (body.IsTracked)
                            {
                                if (ifTrackable(body))
                                {
                                    //isSpeechEnabled = true;
                                    gestureEngine.Body = body;
                                    gestureEngine.StartRecognize();
                                    sweetSpot = true;
                                }

                            }
                            else
                            {
                                //isSpeechEnabled = false;
                                bodies = new Body[6];
                                //Body_Tracking_Highlight(false);
                            }
                        }
                        Body_Tracking_Highlight(sweetSpot);
                    }
                }
            }
            else
            {
                return;
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
            Intro.Visibility = Visibility.Collapsed;
            moveIndex(-1);
        }

        private void swipeLeft()
        {
            print("Swipe Left");
            Intro.Visibility = Visibility.Collapsed;
            moveIndex(1);
        }

        /// <summary>
        /// Checks if the body is in the correct view frustum
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private bool ifTrackable(Body body)
        {
            /*print("Body x = " + body.Joints[JointType.SpineBase].Position.X);
            print("Body y = " + body.Joints[JointType.SpineBase].Position.Y);
            print("Body z = " + body.Joints[JointType.SpineBase].Position.Z);*/

            return (body.Joints[JointType.SpineBase].Position.X <= VIEW_FRUSTUM_X &&
                    body.Joints[JointType.SpineBase].Position.X >= -VIEW_FRUSTUM_X) &&
                   (body.Joints[JointType.SpineBase].Position.Z <= VIEW_FRUSTUM_Z + 0.5 &&
                   body.Joints[JointType.SpineBase].Position.Z >= VIEW_FRUSTUM_Z);
        }

        private Choices getSpeechChoices()
        {
            var directions = new Choices();
            for (int i = 0; i < NAMES.Length; ++i)
            {
                directions.Add(new SemanticResultValue(NAMES[i].ToLower(), NAMES[i].ToUpper()));
            }

            return directions;
        }

        private void setupSpeechRecognition()
        {
            RecognizerInfo ri = TryGetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                var directions = getSpeechChoices();

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(directions);

                var g = new Grammar(gb);

                this.speechEngine.LoadGrammar(g);

                this.speechEngine.SpeechRecognized += this.SpeechRecognized;

                // let the convertStream know speech is going active
                this.convertStream.SpeechActive = true;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                this.speechEngine.SetInputToAudioStream(
                    this.convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                this.speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                print("Speech Recognition not present");
            }
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            print("Recognized");
            // isSpeechEnabled = true;
            print(" isSpeechEnabled? " + isSpeechEnabled);
            if (isSpeechEnabled)
            {
                // Speech utterance confidence below which we treat speech as if it hadn't been heard
                const double ConfidenceThreshold = 0.6;

                Debug.WriteLine("\nConfidence = " + e.Result.Confidence);
                Debug.WriteLine("Speech = " + e.Result.Semantics.Value.ToString() + "\n");
                //Show_Name(e.Result.Semantics.Value.ToString());
                if (e.Result.Confidence >= ConfidenceThreshold)
                {
                    int index = findName(e.Result.Semantics.Value.ToString());
                    if (index != -1)
                    {
                        print("Found");
                        moveToIndex(index);

                    }
                }
            }
        }

        private int findName(string v)
        {
            for (int i = 0; i < NAMES.Length; ++i)
            {
                if (v.ToUpper().Equals(NAMES[i].ToUpper()))
                {
                    return i;
                }
            }
            return -1;
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
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string[] fileArray = Directory.GetFiles((@desktopPath + "\\sciencenterimages\\"), "*.jpg");
            for (int j = 0; j < IMAGES.Length; j++)
            {
                IMAGES[j] = (System.IO.Path.GetFileName(fileArray[j]));
                NAMES[j] = IMAGES[j].Split('_')[0];
            }
            for (int i = 0; i < IMAGES.Length; i++)
            {
                // get the image resources from the xap
                string url = IMAGES[i];
                Image image = new Image();
                //BitmapImage im = new BitmapImage(uri);
                BitmapImage im = new BitmapImage(new Uri(desktopPath + "\\sciencenterimages\\" + url, UriKind.Absolute));
                image.Source = im;

                image.Height = im.Height;
                image.Width = im.Width;
                if (im == null)
                {
                    print("NULL img");
                }

                // add and reposition the image 
                LayoutRoot.Children.Add(image);
                posImage(image, i);
                _images.Add(image);

            }
        }

        // move the index
        private void moveIndex(int value)
        {

            print("Target = " + _target);
            if (value > 0)
            {
                _target = (_target + value) % (_images.Count);
            }
            else
            {
                _target = (_target + _images.Count + value) % (_images.Count);
            }
            int moveToIndx = (int)_target;
            print("image time: " + getTimeInterval(moveToIndx));
            this.slideshowTimer.Stop();
            this.slideshowTimer.Interval = TimeSpan.FromMilliseconds(1000 * getTimeInterval(moveToIndx));
            this.slideshowTimer.Start();

        }

        private void moveToIndex(int value)
        {
            if (!(_target == value))
            {

            }
            print("Target = " + _target);
            moveIndex(value - (int)_target);
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

        private void Show_Name(string name)
        {
            Name_Pop.Visibility = Visibility.Visible;
            Name_Text.Text = name;
        }

        private void Hide_Name()
        {
            Name_Pop.Visibility = Visibility.Collapsed;
            Name_Text.Text = "";

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
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();

            // Save the center position
            _xCenter = this.ActualWidth / 2;
            _yCenter = this.ActualHeight / 2;
        }

        private void Body_Tracking_Highlight(bool tracked)
        {
            if (tracked)
            {
                isSpeechEnabled = true;
                CanvasBorder.BorderBrush = new SolidColorBrush(Colors.MediumSeaGreen);
                //CanvasBorder.BorderThickness = new Thickness(8);
                //LayoutRoot.Background = new SolidColorBrush(Colors.Honeydew);
                if (slideshowTimer.IsEnabled)
                {
                    slideshowTimer.Stop();
                    popupTimer.Start();
                    Intro.Visibility = Visibility.Visible;
                    Popup.Visibility = Visibility.Visible;
                    Greeting.Visibility = Visibility.Visible;
                    Goodbye.Visibility = Visibility.Collapsed;
                    popupTimer.Tick += pop_out;
                }
            }
            else
            {
                isSpeechEnabled = false;
                CanvasBorder.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                //CanvasBorder.BorderThickness = new Thickness(8);
                //LayoutRoot.Background = new SolidColorBrush(Colors.LemonChiffon);
                if (!slideshowTimer.IsEnabled)
                {
                    slideshowTimer.Start();
                    popupTimer.Start();
                    Popup.Visibility = Visibility.Visible;
                    Greeting.Visibility = Visibility.Collapsed;
                    Goodbye.Visibility = Visibility.Visible;
                    popupTimer.Tick += pop_out;
                }
            }

        }

    }

}
