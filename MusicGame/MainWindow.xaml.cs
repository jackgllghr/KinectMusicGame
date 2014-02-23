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
using System.Timers;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;


using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;

namespace MusicGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        private SpeechRecognitionEngine speechEngine;

        Sound a, b, c, applause, backtrack;
        int currTime = 0;
        int len = 8;
        int heldSample;
        
        Sample[] g=new Sample[4];
        Sample[] solutionSamples=new Sample[4];
        Timer time, solutionTimer;
        Track gt, solution;

        ImageSource playImg, pauseImg;
        
        private KinectSensorChooser sensorChooser;
        private InteractionStream _interactionStream;
        //private EventHandler<HandPointerEventArgs> OnGripHand;

        private Skeleton[] _skeletons; //the skeletons 
        private UserInfo[] _userInfos; //the information about the interactive users
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitializeKinect();
           
            KinectRegion.AddHandPointerGripHandler(kRegion, OnHandGrip);
            KinectRegion.AddHandPointerGripReleaseHandler(kRegion, OnHandGripRelease);

            Image guitarIcon = new Image
            {
                Source = new BitmapImage(new Uri("Assets/Icons/guitar.png", UriKind.Relative)),
                Height = 100
            };

            playImg = new BitmapImage(new Uri("Assets/Icons/playButton.png", UriKind.Relative));
            pauseImg = new BitmapImage(new Uri("Assets/Icons/pauseButton.png", UriKind.Relative));      
           
            a=new Sound("Assets/Sounds/GuitarC.wav", "C-Chord", "guitar");
            b=new Sound("Assets/Sounds/GuitarD.wav", "D-Chord", "guitar");
            c=new Sound("Assets/Sounds/GuitarG.wav", "G-Chord", "guitar");
            backtrack = new Sound("Assets/Sounds/drumloop_fast.wav", "BackingTrack", "drums");
            applause=new Sound("Assets/Sounds/applause.wav", "Applause", "cheer");

            g[0] = new Sample(a);
            g[1] = new Sample(b);
            g[2] = new Sample(c);
            g[3] = new Sample(c);


            gt = new Track(8, "guitar", 4, 5);

            gt.addSample(0,g[0]);
            gt.addSample(2,g[1]);
            gt.addSample(4,g[2]);
            gt.addSample(6,g[3]);

            solutionSamples[0] = new Sample(backtrack);
            solutionSamples[1] = new Sample(applause);
            solutionSamples[2] = new Sample(b);
            solutionSamples[3] = new Sample(applause);


            solution = new Track(8, "guitar", 1, 2);
            solution.addSample(0, solutionSamples[0]);
            solution.addSample(2, solutionSamples[1]);
            solution.addSample(4, solutionSamples[2]);
            solution.addSample(6, solutionSamples[3]);
            solutionTimer = new Timer(1000);
            solutionTimer.Elapsed += new ElapsedEventHandler(solutionTimer_Tick);   

            drawIcons(gt);
            //Start the timer
            time = new Timer(1000);
            time.Elapsed += new ElapsedEventHandler(time_Tick);
            time.Start();
            
        }

       
        private void OnHandGrip(object sender, HandPointerEventArgs args)
        {
            args.HandPointer.IsInGripInteraction = true;
            int slot=checkHandForSample(args.HandPointer);

            //Start moving the sample
            if (gt.samples[slot] != null)
            {
                gt.samples[slot].setMoving(true);
                //MessageBox.Show("Slot: " + slot.ToString());
                //gt.samples[slot].getIcon().Margin = new Thickness(, 0, 0, 0);
                heldSample = slot;
            }
        }
        private void OnHandGripRelease(object sender, HandPointerEventArgs args)
        {
            args.HandPointer.IsInGripInteraction = false;
            int slot = checkHandForSample(args.HandPointer);

            if (gt.samples[slot] == null)
            {
                gt.samples[heldSample].setMoving(false);

                gt.addSample(slot, gt.samples[heldSample]);
                gt.removeSample(heldSample);
                gt.samples[slot].getIcon().Margin = new Thickness(slot*101, 0, 0, 0);
            }
        }
        private int checkHandForSample(HandPointer hand)
        {
            //Get Slot
            double x = hand.GetPosition(slot1).X;
            int slot = (int)x / 101;
            return slot;
        }
        private void InitializeKinect(){
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            
            if (this.sensorChooser.Kinect != null)
            {
                _skeletons = new Skeleton[this.sensorChooser.Kinect.SkeletonStream.FrameSkeletonArrayLength];
                _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
                _interactionStream = new InteractionStream(this.sensorChooser.Kinect, new DummyInteractionClient());
                _interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;

                this.sensorChooser.Kinect.DepthFrameReady += SensorOnDepthFrameReady;
                this.sensorChooser.Kinect.SkeletonFrameReady += SensorOnSkeletonFrameReady;
            }
        }
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
       
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    
                    return recognizer;
                }
            }

            return null;
        }
        private SpeechRecognitionEngine CreateSpeechRecogniser()
        {
            //set recognizer info
            RecognizerInfo ri = GetKinectRecognizer();

            //create instance of SRE
            SpeechRecognitionEngine sre;
            sre = new SpeechRecognitionEngine(ri.Id);
            
            //Now we need to add the words we want our program to recognise
            var grammar = new Choices();
            grammar.Add("play");
            grammar.Add("stop");
            grammar.Add("pause");
            grammar.Add("solution");

            //set culture - language, country/region
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            //set up the grammar builder
            var g = new Grammar(gb);
            sre.LoadGrammar(g);

            //Set events for recognizing, hypothesising and rejecting speech
            sre.SpeechRecognized += SreSpeechRecognized;
            sre.SpeechHypothesized += SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;
            return sre;
            
        }
        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //throw new NotImplementedException();
        }
        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < .4)
            {
               //RejectSpeech(e.Result);
            }
            switch (e.Result.Text.ToUpperInvariant())
            {
                case "PLAY":
                    playTrack(gt);
                    break;
                case "STOP":
                case "PAUSE":
                    pauseTrack(gt);
                    break;
                case "SOLUTION":
                    playSolution();
                    break;
                default:
                    break;
            }
        }     
        private void StartAudioListening() 
        {
            //set sensor audio source to variable
            var audioSource = this.sensorChooser.Kinect.AudioSource;
            //Set the beam angle mode - the direction the audio beam is pointing
            //we want it to be set to adaptive
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            //start the audiosource 
            var kinectStream = audioSource.Start();
            //configure incoming audio stream
            speechEngine.SetInputToAudioStream(
                kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            //make sure the recognizer does not stop after completing     
            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            //reduce background and ambient noise for better accuracy
            this.sensorChooser.Kinect.AudioSource.EchoCancellationMode = EchoCancellationMode.None;
            this.sensorChooser.Kinect.AudioSource.AutomaticGainControlEnabled = false;
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

                this.speechEngine = CreateSpeechRecogniser();
                args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                args.NewSensor.SkeletonStream.Enable();
                
                try
                {
                    //args.NewSensor.DepthStream.Range = DepthRange.Near;
                    //args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    //args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                }
                catch (InvalidOperationException)
                {
                    // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    error = true;
                }
            }
            catch (InvalidOperationException)
            {
                error = true;
                // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                // E.g.: sensor might be abruptly unplugged.
            }
            if (!error)
                kRegion.KinectSensor = args.NewSensor;
                StartAudioListening();
        }
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
                    var accelerometerReading = this.sensorChooser.Kinect.AccelerometerGetCurrentReading();
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
                    Width = 100,
                    Height = 100,
                    Margin = new System.Windows.Thickness(101 * i, 0, 0, 0),
                    Fill = Brushes.PaleGreen,
                    Name="slot"+(i+1)
                };
               
                guitarTrack.Children.Add(r);

            }
            guitarTrack.Margin = new System.Windows.Thickness(236,0,0,0);
        }
        private void drawIcons(Track t)
        {
            for (int i = 0; i < t.trackLength; i++) {
                if (t.samples[i] != null)
                {
                    //Image icon = new Image
                    //{
                    //    Source = new BitmapImage(new Uri(t.samples[i].getIcon(), UriKind.Relative)),
                    //    Margin = new System.Windows.Thickness(101*i,0,0,0),
                    //    Height = 100
                    //};
                    Image img = t.samples[i].getIcon();
                    img.Margin = new Thickness(101 * i, 0, 0, 0);
                    guitarTrack.Children.Add(img);
                }
            }
        }
        public void time_Tick(object sender, EventArgs e)
        {     
            if (currTime == len - 1) {
                currTime = 0;
            }
            
            gt.play(currTime);
            currTime++;
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (gt.getPlaying())
            {
                pauseTrack(gt);
            }
            else {
                playTrack(gt);
            }
        }
        private void playTrack(Track t) {
            t.setPlaying(true);
            playButtonImage.Source = pauseImg;
        }
        private void pauseTrack(Track t) {
            t.setPlaying(false);
            playButtonImage.Source = playImg;
        }
        private void solutionTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (currTime == len - 1)
            {
                stopSolution();
            }
            solution.play(currTime);
            currTime++;
        }
        private void SolutionButton_Click(object sender, RoutedEventArgs e)
        {
            playSolution();
        }
        private void playSolution() {
           // time.Stop();
            currTime = 0;
            pauseTrack(gt);
            solutionTimer.Start();
        }
        private void stopSolution() {
            playTrack(gt);
            currTime = 0;
            //time.Start();
            solutionTimer.Stop();
        }
    }
}
