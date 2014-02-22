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
        SoundPlayer player;
        String name, type;
        
        public Sound(
            String soundFile, 
            String soundName,
            String soundType)
            {
                player = new SoundPlayer(soundFile);
               
                name = soundName;
                type = soundType;
            }

        public void play()
        {
            player.Play();
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
