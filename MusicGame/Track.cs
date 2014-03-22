using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicGame
{
    /// <summary>
    /// Contains all data and UI elements associated with a track in the game
    /// </summary>
  public class Track {
      /// <summary>
      /// Boolean that sets there the track is playing/muted
      /// </summary>
      public bool isPlaying { get; set; }
      /// <summary>
      /// The samples array contains the samples and the slot in the array corresponds to the slot in the track the sample occupies
      /// </summary>
      public Sample[] samples { get; set; }
      
      /// <summary>
      /// The length of the track, in number of samples per loop
      /// </summary>
      public int trackLength { get; set; }
      
      /// <summary>
      /// The type of the track (i.e. guitar/bass/drums)
      /// </summary>
      public String type { get; set; }

      /// <summary>
      /// The UI elements of the track
      /// </summary>
      public Canvas trackUI { get; set; }

      /// <summary>
      /// The UI "slots" that the samples go into
      /// </summary>
      public Rectangle[] slots { get; set; }

      /// <summary>
      /// Image object for the mute and unmute icons that appear in the UI
      /// </summary>
      Image muteIcon, volumeIcon;
      
      /// <summary>
      /// A KinectTileButton that is used to set the track to mute or unmute
      /// </summary>
      public Microsoft.Kinect.Toolkit.Controls.KinectTileButton mute { get; set; }
      
      /// <summary>
      /// The constructor takes the length the track needs to be, the type of track, and which order number it is. This is used to set the UI  
      /// </summary>
      /// <param name="len"></param>
      /// <param name="trackType">What type the track is(guitar/bass/drums)</param>
      /// <param name="trackNo">The order number in the collection of tracks</param>
      public Track(int len, String trackType, int trackNo) {
        samples=new Sample[len];

        isPlaying=true;
        trackLength=len;
        type=trackType;

        //Set up the UI Elements
        slots = new Rectangle[len];
        trackUI = new Canvas {
            Height=100, 
            Width=808,
            Margin=new System.Windows.Thickness(0,101*trackNo,0,0)
            
        };
        //Add the UI Rectangle for each slot in the track
        for (int i = 0; i < trackLength; i++)
        {
            slots[i] = new Rectangle
            {
                Width = 100,
                Height = 100,
                Opacity=0.6,
                Margin = new System.Windows.Thickness(101 * i, 0, 0, 0),
                Fill = Brushes.PaleGreen,
                Name = "slot" + (i + 1)
            };
            trackUI.Children.Add(slots[i]);
        }
        
        //Add a mute button to each track
        muteIcon = new Image();
        volumeIcon = new Image();
        muteIcon.Source = new BitmapImage(new Uri("Assets/Icons/muteIconWhite.png", UriKind.Relative));
        volumeIcon.Source = new BitmapImage(new Uri("Assets/Icons/volumeIconWhite.png", UriKind.Relative));
        
        mute = new Microsoft.Kinect.Toolkit.Controls.KinectTileButton
        {
            Margin = new System.Windows.Thickness(101 * trackLength, 0, 0, 0),
            Width = 100,
            Height = 100,
            Foreground = Brushes.White,
            Background = Brushes.RoyalBlue,

        };
        
        mute.Content = volumeIcon;
        mute.Click+=new System.Windows.RoutedEventHandler(MuteButtonClick);
        trackUI.Children.Add(mute);
      }
      /// <summary>
      /// Event handler for the mute button, if hit, a track is set to mute if it's playing or vice versa
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void MuteButtonClick(object sender, EventArgs e)
      {
          MuteTrack();
          if (isPlaying) mute.Content = volumeIcon;
          else mute.Content = muteIcon;
      }
      /// <summary>
      /// Adds the given Sample to the given slot
      /// </summary>
      /// <param name="i">The slot to insert the sample</param>
      /// <param name="s">The sample object that contains audio data</param>
      public void AddSample(int i, Sample s) {
          if (s.type == type)
          {
              samples[i] = s;
              s.isMoving = false;
          }
      }
      /// <summary>
      /// Removes a sample from the track, where i is the slot with the sample to be removed
      /// </summary>
      /// <param name="i">Slot which contains the sample to be removed</param>
      public void RemoveSample(int i) {
        samples[i]=null;
      }
      /// <summary>
      /// Swaps the samples in two slots, useful when a user drops a sample on top of another sample
      /// </summary>
      /// <param name="i">Number of one of the slots containing a sample</param>
      /// <param name="j">Number of one of the slots containing a sample</param>
      public void SwapSamples(int i, int j)
      {
          Sample temp = samples[i];
          samples[i] = samples[j];
          samples[j] = temp;
      }
      /// <summary>
      /// Move a sample from one slot to another
      /// </summary>
      /// <param name="oldSlot">The slot number of the old slot</param>
      /// <param name="newSlot">The slot number of the new slot</param>
      public void MoveSample(int oldSlot, int newSlot)
      {
          AddSample(newSlot, samples[oldSlot]);
          RemoveSample(oldSlot);
          samples[newSlot].icon.Margin = new System.Windows.Thickness(newSlot * 101, 0, 0, 0);
      }
      /// <summary>
      /// Plays the audio data in the sample at the given slot(denoted by time)
      /// </summary>
      /// <param name="time">The slot number will be played</param>
      public void Play(int time) {
        if (isPlaying && samples[time]!=null) {
          samples[time].Play();
        }
      }
      /// <summary>
      /// Sets the current track to muted so the user can isolate tracks audio
      /// </summary>
      public void MuteTrack()
      {
          isPlaying = !isPlaying;
      }
      /// <summary>
      /// Adds an array of samples to a track in random positions
      /// </summary>
      /// <param name="samps">Array of samples</param>
      public void AddSamplesRandomly(Sample[] samps)
      {
          Random r=new Random();
          foreach (var samp in samps)
          {
              //Check if the samples in the array are of the same type
              if (samp.type == type)
              {
                  int i = r.Next(0, 7);
                  bool x = true;
                  while (x)
                  {
                      if (samples[i] == null)
                      {
                          AddSample(i, samp);
                          x = false;
                      }
                      else i = r.Next(0, 7);
                  }
              }
              else
              {
                  //Don't add it to the track
              }
          }
      }
      /// <summary>
      /// Add the icons to the trackUI element in appropriate positions
      /// </summary>
      public void DrawIcons()
      {
          for (int i = 0; i < trackLength; i++)
          {
              if (samples[i] != null)
              {
                  Image img = samples[i].icon;
                  img.Margin = new System.Windows.Thickness(101 * i, 0, 0, 0);
                  trackUI.Children.Add(img);
              }
          }
      }
    }
}
