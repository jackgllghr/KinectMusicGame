using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Resources;
using System.Windows.Controls;
using MusicGame.Properties;
using System.Windows.Media.Imaging;
using WMPLib;

namespace MusicGame
{
    /// <summary>
    /// The Sample class controls the musical samples that the user interacts with. 
    /// Each consists of a sound, name, type and image
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// Icon used in the UI
        /// </summary>
        public Image icon { get; set; }
        /// <summary>
        /// Tells the MainWindow if the sample is in motion
        /// </summary>
        public Boolean isMoving { get; set; }
        /// <summary>
        /// The name of the sample, used for comparing samples
        /// </summary>
        public String name { get; set; }
        /// <summary>
        /// The type of the sample, must match the type of the track
        /// </summary>
        public String type { get; set; }
        /// <summary>
        /// The sound player object that handles audio output
        /// </summary>
        public WindowsMediaPlayer player;

        /// <summary>
        /// The constructor takes 3 args, the path of the WAV audio file, the name of the sample, and what type it is(i.e. guitar/drums/bass)
        /// </summary>
        /// <param name="soundFile">The path to the audio file(Must be ina Windows Media Player supported format)</param>
        /// <param name="soundName">The name of the sample</param>
        /// <param name="soundType">What instrument type it is</param>
        public Sample(
            String soundFile,
            String soundName,
            String soundType)
        {
            player = new WindowsMediaPlayer();
            player.URL = soundFile;

            name = soundName;
            type = soundType;
            isMoving = false;
            SetIcon();
        }
        /// <summary>
        /// This method plays the audio file in the sample
        /// </summary>
        public void Play()
        {
                player.controls.play();
        }

        /// <summary>
        /// This sets the image that the sample will display depending on it's type
        /// If the type name is "guitar", the image will be a guitar icon, etc
        /// </summary>
        void SetIcon()
        {
            if (type.ToLower() == "guitar")
            {
                icon = new Image
                {
                    Source = new BitmapImage(new Uri("Assets/Icons/guitar.png", UriKind.Relative)),
                    Height = 100
                };
                    
            }
            else if (type.ToLower() == "bass")
            {
                icon = new Image
                {
                    Source = new BitmapImage(new Uri("Assets/Icons/bass.png", UriKind.Relative)),
                    Height = 100
                };
            }
            else if (type.ToLower() == "drums")
            {
                icon = new Image
                {
                    Source = new BitmapImage(new Uri("Assets/Icons/drums.png", UriKind.Relative)),
                    Height = 100
                };
            }
        }
    }
}
