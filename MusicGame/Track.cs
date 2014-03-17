using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;


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

      public Canvas trackUI { get; set; }

      public Rectangle[] slots { get; set; }
      /// <summary>
      /// The constructor takes the length the track needs to be and the 
      /// </summary>
      /// <param name="len"></param>
      /// <param name="trackType"></param>
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
      }
      //Add sample to track
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
      public void SwapSamples(int i, int j)
      {
          Sample temp = samples[i];
          samples[i] = samples[j];
          samples[j] = temp;
      }
      public void play(int time) {
        if (isPlaying && samples[time]!=null) {
          samples[time].play();
        }
      }
      public void pause()
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
