using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

using WMPLib;

namespace MusicGame
{
    class Sound
    {
        WindowsMediaPlayer player;
        String name, type;
        
        public Sound(
            String soundFile, 
            String soundName,
            String soundType)
            {
                player = new WindowsMediaPlayer();
                player.URL = soundFile;
                
                name = soundName;
                type = soundType;
            }

        public void play()
        {
            //player.Play();
            player.controls.play();
            
        }
        public void playLooping()
        {
            player.settings.volume = 30;
            player.settings.setMode("loop", true);
        }
        public void stop()
        {
            player.controls.stop();
        }
        public String getName()
        {
            return name;
        }
        public String getType()
        {
            
            return type;
        }
    }
}
