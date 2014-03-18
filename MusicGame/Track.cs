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
  class Track {
      public Boolean isPlaying { get; set; }
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

      Image muteIcon, volumeIcon;
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
        slots = new Rectangle[len];
        trackUI = new Canvas {
            Height=100, 
            Width=808,
            Margin=new System.Windows.Thickness(0,101*trackNo,0,0)
            
        };
        
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
      public void addSample(int i, Sample s) {
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
      public void removeSample(int i) {
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
      /// Plays the audio data in the sample at the given slot(denoted by time)
      /// </summary>
      /// <param name="time">The slot number will be played</param>
      public void play(int time) {
        if (isPlaying && samples[time]!=null) {
          samples[time].play();
        }
      }
      /// <summary>
      /// Sets the current track to pause or muted so the user can isolate tracks
      /// </summary>
      public void MuteTrack()
      {
          isPlaying = !isPlaying;
      }

      public void AddSamplesRandomly(Sample[] samps)
      {
          Random r=new Random();
          foreach (var samp in samps)
          {
              int i = r.Next(0, 7);
              bool x=true;
              while (x)
              {
                  if (samples[i] == null)
                  {
                      addSample(i, samp);
                      x = false;
                  }
                  else i = r.Next(0, 7);
              }
          }
      }
      public void drawIcons()
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
