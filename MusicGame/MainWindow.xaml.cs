using System;
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
        /// <summary>
        /// Speech Recognition engine used to make commands
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        Sound applause;
        /// <summary>
        /// The current time the game's at(from 0-7)
        /// </summary>
        int currTime = 0;
        /// <summary>
        /// The length of the sequencer
        /// </summary>
        int len = 8;
        /// <summary>
        /// The number of regular tracks in the game
        /// </summary>
        int numberOfTracks = 2;
        /// <summary>
        /// The slot and track that the hand has grabbed a sample from
        /// </summary>
        int heldSampleSlot, heldSampleTrack;
        /// <summary>
        /// How many time the solution track has played in it's current run
        /// </summary>
        int solutionPlayCount;
        /// <summary>
        /// The speed of the timer, how many times it ticks in milliseconds
        /// </summary>
        int timerSpeed = 500;
        /// <summary>
        /// Tells if the user is gripping at the moment or not
        /// </summary>
        bool isGripinInteraction = false;
        /// <summary>
        /// Tells if all the tracks are paused
        /// </summary>
        bool isPlaying = true;

        /// <summary>
        /// The guitar samples created
        /// </summary>
        Sample[] guitar=new Sample[4];
        /// <summary>
        /// The drum samples in the game
        /// </summary>
        Sample[] drums = new Sample[6];
       
        /// <summary>
        /// The timers for the regular tracks, the solution tracks and for the animation
        /// </summary>
        Timer time, solutionTimer, animationTimer;
        
        /// <summary>
        /// The tracks which samples are inserted, moved or swapped
        /// </summary>
        Track[] tracks, solutionTracks;
        /// <summary>
        /// The array of seperate frames in the animation
        /// </summary>
        ImageSource[] animation;
        /// <summary>
        /// The current frame counter in the animation
        /// </summary>
        int animationCurrentFrame;

        /// <summary>
        /// Chooses a sensor to be used in the game
        /// </summary>
        private KinectSensorChooser sensorChooser;
        /// <summary>
        /// The interaction stream which allows the Kinect Toolkits Controls to be used
        /// </summary>
        private InteractionStream interactionStream;
        /// <summary>
        /// The stream of color data which has the background removed
        /// </summary>
        private BackgroundRemovedColorStream backgroundRemovedColorStream;
        /// <summary>
        /// The bitmap used to add the background removed color data to the UI
        /// </summary>
        private WriteableBitmap foregroundBitmap;
        /// <summary>
        /// The id of the currently tracked user
        /// </summary>
        private int currentlyTrackedSkeletonId;
        
        /// <summary>
        /// The skeleton data collected from the Kinect
        /// </summary>
        private Skeleton[] skeletons;
        
        /// <summary>
        /// This is where the program begins
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        /// <summary>
        /// Loads all the animation, tracks, samples and Kinect objects into the game
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitializeKinect();
            
            //Load the animation timer, get the frames and start displaying it
            animationCurrentFrame = 0;
            animation = GetAnimationFrames();
            animationTimer = new Timer(33);
            animationTimer.Elapsed += AnimationTimer_Tick;
            animationTimer.Start();

            //Instantiate the tracks and the solution tracks array
            tracks = new Track[numberOfTracks];
            solutionTracks = new Track[numberOfTracks];

            //create all the samples in the game
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

            //Add the samples to the tracks at random
            tracks[0] = new Track(len, "guitar",0);
            tracks[0].AddSamplesRandomly(guitar);

            tracks[1] = new Track(len, "drums", 1);
            tracks[1].AddSamplesRandomly(drums);

            //Add the samples to the solution tracks in the correct order
            solutionTracks[0] = new Track(len, "guitar",0);
            solutionTracks[0].AddSample(0, guitar[0]);
            solutionTracks[0].AddSample(2, guitar[1]);
            solutionTracks[0].AddSample(4, guitar[2]);
            solutionTracks[0].AddSample(6, guitar[3]);

            solutionTracks[1] = new Track(len, "drums", 1);
            solutionTracks[1].AddSample(0, drums[0]);
            solutionTracks[1].AddSample(1, drums[1]);
            solutionTracks[1].AddSample(3, drums[4]);
            solutionTracks[1].AddSample(4, drums[2]);
            solutionTracks[1].AddSample(5, drums[3]);
            solutionTracks[1].AddSample(7, drums[5]);

            //Instantiate the solution tracks timer and it's tick event
            solutionTimer = new Timer(timerSpeed);
            solutionTimer.Elapsed += new ElapsedEventHandler(SolutionTimer_Tick);

            //Update the UI to draw the icons for each regular non-solution track
            foreach (Track t in tracks)
            {
                t.DrawIcons();
                tracksUI.Children.Add(t.trackUI);
            }
            
            //Start the regular tracks timer
            time = new Timer(timerSpeed);
            time.Elapsed += new ElapsedEventHandler(Time_Tick);
            time.Start();
        }
        /// <summary>
        /// On each tick, increment the animation frame to be shown in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimationTimer_Tick(object sender, ElapsedEventArgs e)
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
        
        /// <summary>
        /// Update the UI when the user grips/releases their hand
        /// </summary>
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
        /// <summary>
        /// When the user grips, grab the sample in the slot in which the hand pointer is over
        /// </summary>
        private void OnHandGrip(object sender, HandPointerEventArgs args)
        {
            //Find the track and slot that the user grabbed
            int slot=CheckHandForSlot(args.HandPointer);
            int trackSelected = CheckHandForTrack(args.HandPointer);

            //Start moving that sample and update UI with info message
            if (tracks[trackSelected].samples[slot] != null)
            {
                tracks[trackSelected].samples[slot].isMoving = true;
                heldSampleSlot = slot;
                heldSampleTrack=trackSelected;
                consoleUI.Text = "Grabbed " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") in " + tracks[trackSelected].type + " track"; 
            }
            
        }
        /// <summary>
        /// When the users grip is released, check where the hand is, and swap or move the held sample with the slot the hand is over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnHandGripRelease(object sender, HandPointerEventArgs args)
        {
            //Find which track and slot the user dropped the sample on 
            int slot = CheckHandForSlot(args.HandPointer);
            int trackSelected = CheckHandForTrack(args.HandPointer);

            //if dropped on itself, do nothing
            if(slot==heldSampleSlot){
                //do nothing
                
            }
            //if dropped in an empty slot, move the sample from the old slot to the empty one and update UI
            else if (heldSampleTrack == trackSelected && tracks[trackSelected].samples[slot] == null && tracks[trackSelected].samples[heldSampleSlot] != null)
            {
                tracks[trackSelected].samples[heldSampleSlot].isMoving=false;

                //tracks[trackSelected].AddSample(slot, tracks[trackSelected].samples[heldSampleSlot]);
                //tracks[trackSelected].RemoveSample(heldSampleSlot);
                //tracks[trackSelected].samples[slot].icon.Margin = new Thickness(slot * 101, 0, 0, 0);
                tracks[trackSelected].MoveSample(heldSampleSlot, slot);
                consoleUI.Text = "Dropped " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") in "+tracks[trackSelected].type+" track";
                if (CheckIfWon()) { 
                    Win(); 
                }
            }
            //if dropped in a slot containing a sample, swap those samples and update the UI
            else if (heldSampleTrack == trackSelected && tracks[trackSelected].samples[slot] != null && tracks[trackSelected].samples[heldSampleSlot] != null)
            {
                tracks[trackSelected].SwapSamples(slot, heldSampleSlot);
                consoleUI.Text = "Swapped " + tracks[trackSelected].samples[slot].name + " (slot " + (slot + 1) + ") with "
                    +tracks[trackSelected].samples[heldSampleSlot].name+" (slot "+ (heldSampleSlot+1) +") in " + tracks[trackSelected].type + " track";
                if (CheckIfWon())
                {
                    Win();
                }
            }
            
        }
        /// <summary>
        /// Check the position of the hand on the Y-axis to determine what track it is over
        /// </summary>
        /// <param name="handPointer">A Kinect.Toolkit.Controls.HandPointer object</param>
        /// <returns>The track number</returns>
        private int CheckHandForTrack(HandPointer handPointer)
        {
            double y = handPointer.GetPosition(tracksUI).Y;
            int trackSelected = (int)y / 101;
            return trackSelected;
        }
        /// <summary>
        /// Check the position of the hand on the X-axis to determine what slot it is over
        /// </summary>
        /// <param name="hand">A Kinect.Toolkit.Controls.HandPointer object</param>
        /// <returns>The slot number</returns>
        private int CheckHandForSlot(HandPointer hand)
        {
            double x = hand.GetPosition(tracksUI).X;
            int slot = (int)x / 101;
            return slot;
        }
        /// <summary>
        /// Set up the sensorchooser, create handlers for when a Kinect is changed and handlers for the KinectRegion grip events
        /// </summary>
        private void InitializeKinect()
        {
            sensorChooser = new KinectSensorChooser();
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooserUi.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();

            //Add handlers for the Kinect Grip events
            KinectRegion.AddHandPointerGripHandler(kRegion, OnHandGrip);
            KinectRegion.AddHandPointerGripReleaseHandler(kRegion, OnHandGripRelease);
            KinectRegion.AddQueryInteractionStatusHandler(kRegion, GripHandler);
     
        }
        /// <summary>
        /// Get the recognizer of the Kinect for speech recogniton
        /// </summary>
        /// <returns>Kinects recognizer</returns>
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
        /// <summary>
        /// Create the speech recogniser, adds grammar and sets up the handlers for when speech is recognized
        /// </summary>
        /// <returns>A created SpeechRecognitionEngine with grammar</returns>
        private SpeechRecognitionEngine CreateSpeechRecogniser()
        {
            //set recognizer info
            RecognizerInfo ri = GetKinectRecognizer();

            //create instance of SRE
            SpeechRecognitionEngine sre;
            sre = new SpeechRecognitionEngine(ri.Id);
            
            //We add the words we want our program to recognise
            var grammar = new Choices();
            grammar.Add("play");
            grammar.Add("stop");
            grammar.Add("pause");
            grammar.Add("solution");
            grammar.Add("higher");
            grammar.Add("lower");


            //set culture - language, country/region
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            //set up the grammar builder
            var g = new Grammar(gb);
            sre.LoadGrammar(g);

            //Set events for recognizing, hypothesising and rejecting speech
            sre.SpeechRecognized += SpeechRecognizedHandler;
            return sre;
            
        }
        /// <summary>
        /// If the Kinect hears a command it recognises, it executes the command 
        /// </summary>
        private void SpeechRecognizedHandler(object sender, SpeechRecognizedEventArgs e)
        {
            //if the confidence score is too low, ignore
            if (e.Result.Confidence < .4)
            {
                //RejectSpeech(e.Result);
            }
            else
            {
                switch (e.Result.Text.ToUpperInvariant())
                {
                    case "PLAY":
                        PlayTracks();
                        break;
                    case "STOP":
                    case "PAUSE":
                        PauseTracks();
                        break;
                    case "SOLUTION":
                        PlaySolution();
                        break;
                    case "HIGHER":
                        if (sensorChooser.Kinect.ElevationAngle < 21)
                        {
                            sensorChooser.Kinect.ElevationAngle += 5;
                        }
                        break;
                    case "LOWER":
                        if (sensorChooser.Kinect.ElevationAngle > -21)
                        {
                            sensorChooser.Kinect.ElevationAngle -= 5;
                        }
                        break;
                    default:
                        break;
                }
            }
        }   
        /// <summary>
        /// Begin listening for audio on the Kinect
        /// </summary>
        private void StartAudioListening() 
        {
            //set sensor audio source to variable
            var audioSource = sensorChooser.Kinect.AudioSource;
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
            sensorChooser.Kinect.AudioSource.EchoCancellationMode = EchoCancellationMode.None;
            sensorChooser.Kinect.AudioSource.AutomaticGainControlEnabled = false;
        }
        /// <summary>
        /// When a Kinect sensor is plugged or unplugged, enable/disable sensors as necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
         bool error = false;
         if (args.OldSensor != null)
         {
             try
             {
                 //Disable all streams from the old sensor
                 args.OldSensor.DepthStream.Range = DepthRange.Default;
                 args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                 args.OldSensor.DepthStream.Disable();
                 args.OldSensor.SkeletonStream.Disable();
                 args.OldSensor.ColorStream.Disable();
                 if (null != backgroundRemovedColorStream)
                 {   
                     backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                     backgroundRemovedColorStream.Dispose();
                     backgroundRemovedColorStream = null;
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
                //When a new sensor is plugged in, enable all the streams and set the angle to default
                speechEngine = CreateSpeechRecogniser();
                args.NewSensor.DepthStream.Enable();
                args.NewSensor.SkeletonStream.Enable();
                args.NewSensor.ColorStream.Enable();
                args.NewSensor.ElevationAngle = 0;

                backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                backgroundRemovedColorStream.Enable(args.NewSensor.ColorStream.Format, args.NewSensor.DepthStream.Format);

                consoleUI.Text = "Raise your hand and grab a sample to start!";
                // Allocate space to put the depth, color, and skeleton data we'll receive
                if (null == skeletons)
                {
                    skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    interactionStream = new InteractionStream(args.NewSensor, new DummyInteractionClient());
                   
                    args.NewSensor.AllFramesReady += SensorAllFramesReady;
                    backgroundRemovedColorStream.BackgroundRemovedFrameReady += BackgroundRemovedFrameReadyHandler;
                }
                //Try and enable near mode
                try
                {
                    args.NewSensor.DepthStream.Range = DepthRange.Near;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    
                }
                catch (InvalidOperationException)
                {
                    //Kinect for Xbox 360 devices do not support Near mode, so reset back to default mode.
                    args.NewSensor.DepthStream.Range = DepthRange.Default;
                    args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
              
                }
            }
            catch (InvalidOperationException)
            {
                error = true;
            }
            if (!error)
                //If nothing fails assign the newSensor and start listening for audio
                kRegion.KinectSensor = args.NewSensor;
                StartAudioListening();
        }
    }
        /// <summary>
        /// When depth, color and skeleton frames are ready, process each frame, which will in turn fire the BackgroundRemovedFrameReadyHandler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs args)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == sensorChooser || null == sensorChooser.Kinect || sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = args.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                   
                    }
                }

                using (var colorFrame = args.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = args.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(skeletons);
                        var accelerometerReading = sensorChooser.Kinect.AccelerometerGetCurrentReading();
                        interactionStream.ProcessSkeleton(skeletons, accelerometerReading, skeletonFrame.Timestamp);
                        backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);
                    }
                }
                this.ChooseSkeleton();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        /// <summary>
        /// When the background removed frame is ready, create a Bitmap and add it to the UI
        /// </summary>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs args)
        {
            using (var backgroundRemovedFrame = args.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == foregroundBitmap || foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                        // Set the image we display to point to the bitmap which contains the background removed image data
                        MaskedColor.Source = foregroundBitmap;
                    }

                    // Write the pixel data into our bitmap
                    foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, foregroundBitmap.PixelWidth, foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        foregroundBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
        /// <summary>
        /// Choose which skeleton in the scene to track based on how close they are
        /// </summary>
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this.skeletons)
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
        /// <summary>
        /// On each tick of the regular track timer, the sample at the current time is played and the UI is updated
        /// </summary>
        public void Time_Tick(object sender, EventArgs e)
        {     
            //Reset current Time if it runs over the track
            if (currTime == len) {
                currTime = 0;
            }
            //Update the UI to show which samples are being playing at the current time
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
            //Play the sample at that time
            foreach (Track t in tracks)
            {
                t.Play(currTime);
            }
            currTime++;
        }
        /// <summary>
        /// Loops through all the tracks and compares each track with it's respective solution track
        /// </summary>
        /// <returns>True if the user has won, otherwise false</returns>
        private bool CheckIfWon()
        {
            //loop through all the tracks
            for (int i = 0; i < numberOfTracks;i++ )
            {
                if (CompareTracks(tracks[i], solutionTracks[i]))
                {
                    //tracks are a match, do nothing, check the rest
                }
                else return false;
            }
            //if all are matched, return true
            return true;
        }
        /// <summary>
        /// Compares two track by comparing each sample against its respective sample in the opposite track. Compares sample using their name as an identifier
        /// </summary>
        /// <param name="t1">First track to be compared, a regular track</param>
        /// <param name="t2">Second track to be compared, a solution track</param>
        /// <returns></returns>
        private bool CompareTracks(Track t1, Track t2){
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
        /// <summary>
        /// Event that fires when the play button is hit. Plays or pauses all the regular tracks depending on the current state
        /// </summary>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                PauseTracks();
                isPlaying = false;
            }
            else
            {
                PlayTracks();
                isPlaying = true;
            }
        }
        /// <summary>
        /// Plays the regular tracks
        /// </summary>
        private void PlayTracks(){
            //start the timer for regular tracks to get them playing
            time.Start();

            //Update UI of the play button
            Dispatcher.Invoke((Action)delegate()
            {
                playButton.Content = "Pause";
            });
            
        }
        /// <summary>
        /// Pauses the regular tracks 
        /// </summary>
        private void PauseTracks() {
         
            //Stop the timer for regular tracks to stop them playing
            time.Stop();

            //Update UI on the play button
            Dispatcher.Invoke((Action)delegate()
            {
                playButton.Content = "Play";
            });
            
        }
        /// <summary>
        /// Event that happens each time the solution timer ticks. Plays a sample from the solution track and updates the UI
        /// </summary>
        private void SolutionTimer_Tick(object sender, ElapsedEventArgs e)
        {
            //if it's played the full solution, stop playing it and return to playing the regular tracks
            if (currTime == len)
            {
                currTime = 0;
                solutionPlayCount++;
            }
            if(solutionPlayCount<2){
                //Loop through the solution tracks and play them
                for(int i=0;i<numberOfTracks;i++)
                {
                    solutionTracks[i].Play(currTime);

                    //Update UI to show the current position of the solution timer
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
            else 
            { 
                StopSolution(); 
            }
            
        }
        /// <summary>
        /// Event fired when the solution button is clicked. Plays the solution
        /// </summary>
        private void SolutionButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate {
                SolutionButton.Content = "The solution is playing";
                consoleUI.Text = "The solution track is playing, listen carefully to the sounds";
            });
           PlaySolution();
        }
        /// <summary>
        /// Plays the solution. Starts the solution timer and pauses all the other tracks 
        /// </summary>
        private void PlaySolution() {
            Dispatcher.Invoke((Action)delegate() {
                if (currTime < len)
                {
                    foreach (Track t in tracks)
                    {
                        t.slots[currTime].Fill = Brushes.PaleGreen;
                    }
                }
            });
            currTime = 0;
            PauseTracks();
            solutionTimer.Start();
        }
        /// <summary>
        /// Stops the solution. Stops the solution timer and sets the regular tracks to playing
        /// </summary>
        private void StopSolution() {
            solutionTimer.Stop();
            currTime = 0;
            solutionPlayCount = 0;
            Dispatcher.Invoke((Action)delegate
            {
                SolutionButton.Content = "Play Solution";
                consoleUI.Text = "Now try and rearrange the samples to match that solution";
            });
               
            PlayTracks();
        }
        /// <summary>
        /// Activates the win state. Updates the UI with a congratulatory message and starts an audience applause playing
        /// </summary>
        private void Win() {
            time.Stop();
            consoleUI.Text = "Congrats you won!!";
            applause = new Sound("Assets/Sounds/applause.wav", "Applause", "cheer");
            applause.playLooping();
        }
        /// <summary>
        /// Loads all the animation frames into an array
        /// </summary>
        /// <returns>All animation frames as an array of ImageSources</returns>
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
