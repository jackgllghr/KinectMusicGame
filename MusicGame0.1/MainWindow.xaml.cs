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

using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;

namespace MusicGame
{
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;
        private KinectSensor _sensor;  //The Kinect Sensor the application will use
        private InteractionStream _interactionStream;

        private Skeleton[] _skeletons; //the skeletons 
        private UserInfo[] _userInfos; //the information about the interactive users

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Loaded += OnLoaded;
                
                //Sound s = new Sound("C:/GuitarC.wav", "Guitar", "guitar");
                //s.play();
                
                drawTrack(16);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                this.sensorChooser = new KinectSensorChooser();
                this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
                this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
                this.sensorChooser.Start();
                _sensor = sensorChooser.Kinect;
            }
            catch (Exception)
            {

                throw;
            }

            /*if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // this is just a test, so it only works with one Kinect, and quits if that is not available.
            _sensor = KinectSensor.KinectSensors.FirstOrDefault();
            if (_sensor == null)
            {
                MessageBox.Show("No Kinect Sensor detected!");
                Close();
                return;
            }*/

            _skeletons = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
            _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];


            _sensor.DepthStream.Range = DepthRange.Default;
            _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

            _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            _sensor.SkeletonStream.EnableTrackingInNearRange = false;
            _sensor.SkeletonStream.Enable();

            _interactionStream = new InteractionStream(_sensor, new DummyInteractionClient());
            _interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;

            _sensor.DepthFrameReady += SensorOnDepthFrameReady;
            _sensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;

            _sensor.Start();


        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {

            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                        //error = true;
                    }

                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
            if (!error)
                kRegion.KinectSensor = args.NewSensor;
            else if (error)
                MessageBox.Show("Error in Calibration");
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            using (SkeletonFrame skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                try
                {
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    var accelerometerReading = _sensor.AccelerometerGetCurrentReading();
                    _interactionStream.ProcessSkeleton(_skeletons, accelerometerReading, skeletonFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // SkeletonFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }

        private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            using (DepthImageFrame depthFrame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;

                try
                {
                    _interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // DepthFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }

        private Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();
        private Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();

        private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs args)
        {
            using (var iaf = args.OpenInteractionFrame()) //dispose as soon as possible
            {
                if (iaf == null)
                    return;

                iaf.CopyInteractionDataTo(_userInfos);
            }

            StringBuilder dump = new StringBuilder();

            var hasUser = false;
            foreach (var userInfo in _userInfos)
            {
                var userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                    continue;

                hasUser = true;
                dump.AppendLine("User ID = " + userID);
                dump.AppendLine("  Hands: ");
                var hands = userInfo.HandPointers;
                if (hands.Count == 0)
                    dump.AppendLine("    No hands");
                else
                {
                    foreach (var hand in hands)
                    {
                        var lastHandEvents = hand.HandType == InteractionHandType.Left
                                                 ? _lastLeftHandEvents
                                                 : _lastRightHandEvents;

                        if (hand.HandEventType != InteractionHandEventType.None)
                            lastHandEvents[userID] = hand.HandEventType;

                        var lastHandEvent = lastHandEvents.ContainsKey(userID)
                                                ? lastHandEvents[userID]
                                                : InteractionHandEventType.None;

                        dump.AppendLine();
                        dump.AppendLine("    HandType: " + hand.HandType);
                        dump.AppendLine("    HandEventType: " + hand.HandEventType);
                        dump.AppendLine("    LastHandEventType: " + lastHandEvent);
                        dump.AppendLine("    IsActive: " + hand.IsActive);
                        dump.AppendLine("    IsPrimaryForUser: " + hand.IsPrimaryForUser);
                        dump.AppendLine("    IsInteractive: " + hand.IsInteractive);
                        dump.AppendLine("    PressExtent: " + hand.PressExtent.ToString("N3"));
                        dump.AppendLine("    IsPressed: " + hand.IsPressed);
                        dump.AppendLine("    IsTracked: " + hand.IsTracked);
                        dump.AppendLine("    X: " + hand.X.ToString("N3"));
                        dump.AppendLine("    Y: " + hand.Y.ToString("N3"));
                        dump.AppendLine("    RawX: " + hand.RawX.ToString("N3"));
                        dump.AppendLine("    RawY: " + hand.RawY.ToString("N3"));
                        dump.AppendLine("    RawZ: " + hand.RawZ.ToString("N3"));
                    }
                }

                tb.Text = dump.ToString();
            }

            if (!hasUser)
                tb.Text = "No user detected.";

        }
        private void drawTrack(int len)
        {
            for (int i = 0; i < len; i++)
            {
                Rectangle r = new Rectangle
                {
                    Width = 80,
                    Height = 80,
                    Margin = new System.Windows.Thickness(81 * i, 0, 0, 0),
                    Fill = Brushes.PaleGreen
                };
                tracks.Children.Add(r);

            }
        }

        private void tracks_KeyUp(object sender, KeyEventArgs e)
        {
          

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Sound s = new Sound("C:/GuitarC.wav", "Guitar", "guitar");
            s.play();
        }

    }
}
