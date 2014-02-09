using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;

namespace MusicGame
{
    class Sample
    {
        public float x, y, size, centerX, centerY;
        String icon;
        Sound s;
        Boolean isMoving = false;
        Boolean isPlaying = true;
        

        //Constructers
        Sample(Sound sound)
        {
            s = sound;
            x = 0;
            y = 0;

            size = 1;
            //setIcon();
        }
        Sample(Sound sound, float newX, float newY, float slotSize)
        {
            s = sound;
            //setIcon();

            x = newX;
            y = newY;

            size = slotSize;

            centerX = x + (slotSize / 2);
            centerY = y + (slotSize / 2);
        }
        Sample(Sound sound, float newX, float newY, float slotSize, String i)
        {
            s = sound;
            //setIcon();

            x = newX;
            y = newY;
            icon = i;
            size = slotSize;
            
            centerX = x + (slotSize / 2);
            centerY = y + (slotSize / 2);
        }

        //Sound methods
        public void play()
        {
            if (isPlaying)
            {
                s.play();
            }
        }

        //Setters
        //void setIcon()
        //{
        //    if (s.getType().toLowerCase() == "guitar")
        //    {
        //        Image img = loadImage("Icons/guitar.png");
        //    }
        //    else if (s.getType().toLowerCase() == "bass")
        //    {
        //        icon = loadImage("Icons/bass.png");
        //    }
        //    else if (s.getType().toLowerCase() == "drums")
        //    {
        //        icon = loadImage("Icons/drums.png");
        //    }
        //}
        public void setXY(float newX, float newY)
        {
            x = newX;
            y = newY;
        }
        public void setPlaying()
        {
            isPlaying = !isPlaying;
        }
        public void setSize(float newSize)
        {
            size = newSize;
        }
        public String getType()
        {
            return s.type;
        }
        public Boolean getMoving() {
            return isMoving;
        }
        public void setMoving(Boolean state)
        {
            isMoving = state ;
        }
        //Drawing method
        /*public void drawIcon()
        {
            if (isMoving)
            {
                imageMode(CENTER);
                image(icon, mouseX, mouseY, size, size);
                imageMode(CORNER);
            }
            else
            {
                image(icon, x, y, size, size);
            }//print(s.name+": IconX="+x+" IconY="+y+"  Size="+size+"\n");
        }*/
        public void move(float newX, float newY)
        {
            if (true)
            {
                //print(x);
            }

        }

    }
}
