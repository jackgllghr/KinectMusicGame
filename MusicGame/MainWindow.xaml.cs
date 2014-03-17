﻿using System;
using System.IO;
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

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

using System.Diagnostics;

namespace MusicGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        private SpeechRecognitionEngine speechEngine;

        Sound applause;
        int currTime = 0;
        int len = 8;
        int numberOfTracks = 2;
        int heldSampleSlot, heldSampleTrack;
        int timerSpeed = 500;
        bool isGripinInteraction = false;

        Sample[] guitar=new Sample[4];
        Sample[] drums = new Sample[6];
       
        Timer time, solutionTimer, animationTimer;
        
        Track[] tracks, solutionTracks;

        ImageSource playImg, pauseImg;
        ImageSource[] animation;
        int animationCurrentFrame;

        //Rectangle[] slots=new Rectangle[8];
        private KinectSensorChooser sensorChooser;
        private InteractionStream _interactionStream;
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        private WriteableBitmap foregroundBitmap;
        private int currentlyTrackedSkeletonId;
        
        private Skeleton[] _skeletons; //the skeletons 
        private UserInfo[] _userInfos; //the information about the interactive users
        
        /// <summary>
        /// This is where the program begins
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitializeKinect();
            
            animationCurrentFrame = 0;
            animation = GetAnimationFrames();
            animationTimer = new Timer(33);
            animationTimer.Elapsed += animationTimer_Tick;
            animationTimer.Start();

            KinectRegion.AddHandPointerGripHandler(kRegion, OnHandGrip);
            KinectRegion.AddHandPointerGripReleaseHandler(kRegion, OnHandGripRelease);
            KinectRegion.AddQueryInteractionStatusHandler(kRegion, GripHandler);
            playImg = new BitmapImage(new Uri("Assets/Icons/playButton.png", UriKind.Relative));
            pauseImg = new BitmapImage(new Uri("Assets/Icons/pauseButton.png", UriKind.Relative));      
           
            //backtrack = new Sound("Assets/Sounds/drumloop_fast.wav", "BackingTrack", "drums");
            tracks = new Track[numberOfTracks];
            solutionTracks = new Track[numberOfTracks];

            guitar[0] = new Sample("Assets/Sounds/GuitarC.wav", "C-Chord", "guitar");
            guitar[1] = new Sample("Assets/Sounds/GuitarD.wav", "D-Chord", "guitar");
            guitar[2] = new Sample("Assets/Sounds/GuitarG.wav", "G-Chord", "guitar");
            guitar[3] = new Sample("Assets/Sounds/GuitarG.wav", "G-Chord", "guitar");

            drums[0] = new Sample("Assets/Sounds/hihat.wav", "Hi-Hat", "drums");
            drums[1] = new Sample("Assets/Sounds/hihat.wav", "Hi-Hat", "drums");
            drums[2] = new Sample("Assets/Sounds/hihat.wav", "Hi-Hat", "drums");
            drums[3] = new Sample("Assets/Sounds/hihat.wav", "Hi-Hat", "drums");
            drums[4] = new Sample("Assets/Sounds/snare.wav", "Snare", "drums");
            drums[5] = new Sample("Assets/Sounds/snare.wav", "Snare", "drums");

            tracks[0] = new Track(len, "guitar",0);
            tracks[0].AddSamplesRandomly(guitar);

            tracks[1] = new Track(len, "drums", 1);
            tracks[1].AddSamplesRandomly(drums);

            solutionTracks[0] = new Track(len, "guitar",0);
            solutionTracks[0].addSample(0, guitar[0]);
            solutionTracks[0].addSample(2, guitar[1]);
            solutionTracks[0].addSample(4, guitar[2]);
            solutionTracks[0].addSample(6, guitar[3]);

            solutionTracks[1] = new Track(len, "drums", 1);
            solutionTracks[1].addSample(0, drums[0]);
            solutionTracks[1].addSample(1, drums[1]);
            solutionTracks[1].addSample(3, drums[4]);
            solutionTracks[1].addSample(4, drums[2]);
            solutionTracks[1].addSample(5, drums[3]);
            solutionTracks[1].addSample(7, drums[5]);

            solutionTimer = new Timer(timerSpeed);
            solutionTimer.Elapsed += new ElapsedEventHandler(solutionTimer_Tick);
            //drawTrack(8);
            //drawIcons(gt);

            foreach (Track t in tracks)
            {
                t.drawIcons();
                tracksUI.Children.Add(t.trackUI);
                
            }
            //backtrack.playLooping();

            //Start the timer
            time = new Timer(timerSpeed);
            time.Elapsed += new ElapsedEventHandler(time_Tick);
            time.Start();
        }

        private void animationTimer_Tick(object sender, ElapsedEventArgs e)
        {
                Dispatcher.Invoke((Action)delegate(){
                        Concert.Source = animation[animationCurrentFrame];
                });
                animationCurrentFrame++;
                if (animationCurrentFrame == 97)
                {
                    animationCurrentFrame = 0;
                }
           
        }
        

        private void GripHandler(object sender, QueryInteractionStatusEventArgs handPointerEventArgs)
        {

            //If a grip detected change the cursor image to grip
            if (handPointerEventArgs.HandPointer.HandEventType == HandEventType.Grip)
            {
                isGripinInteraction = true;
                handPointerEventArgs.IsInGripInteraction = true;
            }

           //If Grip Release detected change the cursor image to open
            else if (handPointerEventArgs.HandPointer.HandEventType == HandEventType.GripRelease)
            {
                isGripinInteraction = false;
                handPointerEventArgs.IsInGripInteraction = false;
            }

            //If no change in state do not change the cursor
            else if (handPointerEventArgs.HandPointer.HandEventType == HandEventType.None)
            {
                handPointerEventArgs.IsInGripInteraction = isGripinInteraction;
            }

            handPointerEventArgs.Handled = true;
        }
        private void OnHandGrip(object sender, HandPointerEventArgs args)
        {

            //args.HandPointer.IsInGripInteraction = true;
            int slot=checkHandForSlot(args.HandPointer);
            int trackSelected = CheckHandForTrack(args.HandPointer);


            //Start moving the sample
            if (tracks[trackSelected].samples[slot] != null)
            {
                tracks[trackSelected].samples[slot].isMoving = true;
                //MessageBox.Show("Slot: " + slot.ToString());
                //gt.samples[slot].getIcon().Margin = new Thickness(, 0, 0, 0);
                heldSampleSlot = slot;
                heldSampleTrack=trackSelected;
                consoleUI.Text = "Grabbed " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") in " + tracks[trackSelected].type + " track"; 
            }
            
        }
        private void OnHandGripRelease(object sender, HandPointerEventArgs args)
        {
            //args.HandPointer.IsInGripInteraction = false;
            int slot = checkHandForSlot(args.HandPointer);
            int trackSelected = CheckHandForTrack(args.HandPointer);

            if(slot==heldSampleSlot){
                //do nothing
                
            }

            else if (heldSampleTrack == trackSelected && tracks[trackSelected].samples[slot] == null && tracks[trackSelected].samples[heldSampleSlot] != null)
            {
                tracks[trackSelected].samples[heldSampleSlot].isMoving=false;

                tracks[trackSelected].addSample(slot, tracks[trackSelected].samples[heldSampleSlot]);
                tracks[trackSelected].removeSample(heldSampleSlot);
                tracks[trackSelected].samples[slot].icon.Margin = new Thickness(slot * 101, 0, 0, 0);
                consoleUI.Text = "Dropped " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") in "+tracks[trackSelected].type+" track";
                if (checkIfWon()) { 
                    Win(); 
                }
            }
            else if (heldSampleTrack == trackSelected && tracks[trackSelected].samples[slot] != null && tracks[trackSelected].samples[heldSampleSlot] != null)
            {
                tracks[trackSelected].SwapSamples(slot, heldSampleSlot);
                consoleUI.Text = "Swapped " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") with "
                    +tracks[trackSelected].samples[heldSampleSlot].name+" (slot "+ (heldSampleSlot+1) +") in " + tracks[trackSelected].type + " track";
                if (checkIfWon())
                {
                    Win();
                }
            }
            
        }

        private int CheckHandForTrack(HandPointer handPointer)
        {
            double y = handPointer.GetPosition(tracksUI).Y;
            int trackSelected = (int)y / 101;
            return trackSelected;
        }
        private int checkHandForSlot(HandPointer hand)
        {
            //Get Slot
            double x = hand.GetPosition(tracksUI).X;
            int slot = (int)x / 101;
            return slot;
        }
        
        private void InitializeKinect()
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
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
            //grammar.Add("higher");
            //grammar.Add("lower");


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
                    playTracks();
                    break;
                case "STOP":
                case "PAUSE":
                    pauseTracks();
                    break;
                case "SOLUTION":
                    playSolution();
                    break;
                //case "HIGHER":
                //    if (sensorChooser.Kinect.ElevationAngle < 21)
                //    {
                //        sensorChooser.Kinect.ElevationAngle += 5;
                //    }
                //    break;
                //case "LOWER":
                //    if (sensorChooser.Kinect.ElevationAngle > -21)
                //    {
                //        sensorChooser.Kinect.ElevationAngle -= 5;
                //    }
                //    break;
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
                 args.OldSensor.ColorStream.Disable();

                 // Create the background removal stream to process the data and remove background, and initialize it.
                 if (null != this.backgroundRemovedColorStream)
                 {
                     
                     this.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                     this.backgroundRemovedColorStream.Dispose();
                     this.backgroundRemovedColorStream = null;
                 }
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
                args.NewSensor.DepthStream.Enable();
                args.NewSensor.SkeletonStream.Enable();
                args.NewSensor.ColorStream.Enable();
                args.NewSensor.ElevationAngle = 0;

                backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                backgroundRemovedColorStream.Enable(args.NewSensor.ColorStream.Format, args.NewSensor.DepthStream.Format);

                consoleUI.Text = "Raise your hand and grab a sample to start!";
                // Allocate space to put the depth, color, and skeleton data we'll receive
                if (null == this._skeletons)
                {
                    _skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    _userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
                    _interactionStream = new InteractionStream(args.NewSensor, new DummyInteractionClient());
                    //_interactionStream.InteractionFrameReady += InteractionStreamOnInteractionFrameReady;

                    //args.NewSensor.DepthFrameReady += SensorOnDepthFrameReady;
                    //args.NewSensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;
                    //args.NewSensor.ColorFrameReady += SensorOnColorImageFrameReady;
                    args.NewSensor.AllFramesReady += SensorAllFramesReady;
                    backgroundRemovedColorStream.BackgroundRemovedFrameReady += BackgroundRemovedFrameReadyHandler;
                }

                // Add an event handler to be called when the background removed color frame is ready, so that we can
                // composite the image and output to the app
                
                // Add an event handler to be called whenever there is new depth frame data
                //args.NewSensor.AllFramesReady += this.SensorAllFramesReady;

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
   
//        private Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();
 //       private Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();
        //private void InteractionStreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs args)
        //{
        //    using (var iaf = args.OpenInteractionFrame()) //dispose as soon as possible
        //    {
        //        if (iaf == null)
        //            return;

        //        iaf.CopyInteractionDataTo(_userInfos);
        //    }
        //    StringBuilder dump = new StringBuilder();

        //    var hasUser = false;
        //    foreach (var userInfo in _userInfos)
        //    {
        //        var userID = userInfo.SkeletonTrackingId;
        //        if (userID == 0)
        //            continue;

        //        hasUser = true;
        //        dump.AppendLine("User ID = " + userID);
        //        dump.AppendLine("  Hands: ");
        //        var hands = userInfo.HandPointers;
        //        if (hands.Count == 0)
        //            dump.AppendLine("    No hands");
        //        else
        //        {
        //            foreach (var hand in hands)
        //            {
        //                var lastHandEvents = hand.HandType == InteractionHandType.Left
        //                                         ? _lastLeftHandEvents
        //                                         : _lastRightHandEvents;

        //                if (hand.HandEventType != InteractionHandEventType.None)
        //                    lastHandEvents[userID] = hand.HandEventType;

        //                var lastHandEvent = lastHandEvents.ContainsKey(userID)
        //                                        ? lastHandEvents[userID]
        //                                        : InteractionHandEventType.None;
                        
        //                dump.AppendLine();
        //                dump.AppendLine("    HandType: " + hand.HandType);
        //                dump.AppendLine("    HandEventType: " + hand.HandEventType);
        //                dump.AppendLine("    LastHandEventType: " + lastHandEvent);
        //                dump.AppendLine("    IsActive: " + hand.IsActive);
        //                dump.AppendLine("    IsPrimaryForUser: " + hand.IsPrimaryForUser);
        //                dump.AppendLine("    IsInteractive: " + hand.IsInteractive);
        //                dump.AppendLine("    PressExtent: " + hand.PressExtent.ToString("N3"));
        //                dump.AppendLine("    IsPressed: " + hand.IsPressed);
        //                dump.AppendLine("    IsTracked: " + hand.IsTracked);
        //                dump.AppendLine("    X: " + hand.X.ToString("N3"));
        //                dump.AppendLine("    Y: " + hand.Y.ToString("N3"));
        //                dump.AppendLine("    RawX: " + hand.RawX.ToString("N3"));
        //                dump.AppendLine("    RawY: " + hand.RawY.ToString("N3"));
        //                dump.AppendLine("    RawZ: " + hand.RawZ.ToString("N3"));
                        
        //            }
        //        }

        //        consoleUI.Text = dump.ToString();
        //    }

        //    if (!hasUser)
        //        consoleUI.Text = "No user detected.";

        //}

        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs args)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = args.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        _interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                   
                    }
                }

                using (var colorFrame = args.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = args.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(_skeletons);
                        var accelerometerReading = this.sensorChooser.Kinect.AccelerometerGetCurrentReading();
                        _interactionStream.ProcessSkeleton(_skeletons, accelerometerReading, skeletonFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessSkeleton(_skeletons, skeletonFrame.Timestamp);
                    }
                }
                this.ChooseSkeleton();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs args)
        {
            using (var backgroundRemovedFrame = args.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == this.foregroundBitmap || this.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || this.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        this.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.MaskedColor.Source = this.foregroundBitmap;
                    }

                    // Write the pixel data into our bitmap
                    this.foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, this.foregroundBitmap.PixelWidth, this.foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        this.foregroundBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this._skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeleton);
                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }

        //private void drawTrack(Track t)
        //{
        //    for (int i = 0; i < len; i++)
        //    {
        //        slots[i] = new Rectangle
        //        {
        //            Width = 100,
        //            Height = 100,
        //            Margin = new System.Windows.Thickness(101 * i, 0, 0, 0),
        //            Fill = Brushes.PaleGreen,
        //            Name="slot"+(i+1)
        //        };
               
        //        guitarTrack.Children.Add(slots[i]);

        //    }
        //}
        
        public void time_Tick(object sender, EventArgs e)
        {     
            //Reset current Time if it runs over the track
            if (currTime == len) {
                currTime = 0;
            }
            //Update the UI on each tick
            Dispatcher.Invoke((Action)delegate() {
                foreach (Track t in tracks)
                {
                    t.slots[currTime].Fill = Brushes.PaleVioletRed;
                    if (currTime == 0)
                    {
                        t.slots[len - 1].Fill = Brushes.PaleGreen;
                    }
                    else t.slots[currTime - 1].Fill = Brushes.PaleGreen;
                }
            });
            foreach (Track t in tracks)
            {
                t.play(currTime);
            }
            currTime++;
        }
       
        private bool checkIfWon()
        {
            for (int i = 0; i < numberOfTracks;i++ )
            {
                if (compareTracks(tracks[i], solutionTracks[i]))
                {
                    //match, do nothing
                }
                else return false;
            }
            return true;
        }
        private bool compareTracks(Track t1, Track t2){
            for (int i = 0; i < len; i++)
            {
                //If both slots are empty, ignore
                if (t1.samples[i] == null && t2.samples[i] == null) {}
                //If one is empty, not a match
                else if((t1.samples[i]!=null && t2.samples[i]==null)||(t1.samples[i]==null && t2.samples[i]!=null)){
                    return false;
                }
                //If both are filled, compare them
                else if (t1.samples[i] != null && t2.samples[i] != null)
                {
                    //If they are the same sound, move on to the next
                    if (t1.samples[i].name.Equals(t2.samples[i].name)) { }
                    else return false;
                }
                else return false;
            }
            return true;
        }
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (tracks[0].isPlaying)
            {
                pauseTracks();
            }
            else
            {
                playTracks();
            }
        }
        private void playTracks(){
            try
            {

                time.Start();
                foreach (Track t in tracks)
                {
                    t.isPlaying = true;
                }
                
                Dispatcher.Invoke((Action)delegate()
                {
                    //playButtonImage.Source = pauseImg;
                    playButton.Content = "Pause";
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }
        private void pauseTracks() {
            try
            {
                time.Stop();
                foreach (Track t in tracks)
                {
                    t.isPlaying = false;
                }
                Dispatcher.Invoke((Action)delegate()
                {
                    //playButtonImage.Source = playImg;
                    playButton.Content = "Play";
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }
        private void solutionTimer_Tick(object sender, ElapsedEventArgs e)
        {
            if (currTime == len)
            {
                stopSolution();
            }
            else
            {
                for(int i=0;i<numberOfTracks;i++)
                {
                    solutionTracks[i].play(currTime);
                    //Update UI
                    Dispatcher.Invoke((Action)delegate()
                    {
                        tracks[i].slots[currTime].Fill = Brushes.RoyalBlue;
                        if (currTime == 0)
                        {
                            tracks[i].slots[len - 1].Fill = Brushes.PaleGreen;
                        }
                        else tracks[i].slots[currTime - 1].Fill = Brushes.PaleGreen;

                    });
                }
                currTime++;
            }
        }
        private void SolutionButton_Click(object sender, RoutedEventArgs e)
        {
            playSolution();
        }
        private void playSolution() {
            Dispatcher.Invoke((Action)delegate() {
                foreach (Track t in tracks) {
                    t.slots[currTime].Fill = Brushes.PaleGreen;
                }
            });
            currTime = 0;
            pauseTracks();
            solutionTimer.Start();
        }
        private void stopSolution() {
            playTracks();
            currTime = 0;
            solutionTimer.Stop();
        }
        private void Win() {
            time.Stop();
            //backtrack.stop();
            consoleUI.Text = "Congrats you won!!";
            //pauseTracks();
            applause = new Sound("Assets/Sounds/applause.wav", "Applause", "cheer");
            applause.playLooping();
        }
        private ImageSource[] GetAnimationFrames()
        {
            try
            {
                ImageSource[] loop = new ImageSource[98];
                for (int i = 1; i < 10; i++)
                {
                    loop[i] = new BitmapImage(new Uri("Assets/Animation/Rock Concert Crowd HD loop 0" + i+".jpg", UriKind.Relative));
                }
                for (int i = 10; i < 98; i++)
                {
                    loop[i] = new BitmapImage(new Uri("Assets/Animation/Rock Concert Crowd HD loop " + i+".jpg", UriKind.Relative));
                }
                return loop;
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }

    }
}
