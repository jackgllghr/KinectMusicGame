﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Resources;
using System.Windows.Controls;
using MusicGame.Properties;

namespace MusicGame
{
    class Sample
    {
        public float x, y, size, centerX, centerY;
        String icon;
        Sound s;
        Boolean isMoving = false;
        Boolean isPlaying = true;
        //private Image img;        
        String img;
        //Constructers
        public Sample(Sound sound)
        {
            s = sound;
            x = 0;
            y = 0;

            size = 1;
            setIcon();
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
                s.play();
        }

        //Setters
        void setIcon()
        {
            if (s.getType().ToLower() == "guitar")
            {
                //img = Image.FromFile("Assets/Icons/guitar.png");
                img = "Assets/Icons/guitar.png";
            }
            else if (s.getType().ToLower() == "bass")
            {
                //img = Image.FromFile("Assets/Icons/bass.png");
            }
            else if (s.getType().ToLower() == "drums")
            {
                //img = Image.FromFile("Assets/Icons/drums.png");
            }
        }
        public String getIcon() {
            return img;
        }
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
            return s.getType();
        }
        public Boolean getMoving() {
            return isMoving;
        }
        public void setMoving(Boolean state)
        {
            isMoving = state ;
        }
        //Drawing method
        public void drawIcon()
        {
            if (isMoving)
            {
                /*imageMode(CENTER);
                image(icon, mouseX, mouseY, size, size);
                imageMode(CORNER);*/
            }
            else
            {
                //image(icon, x, y, size, size);
               
            }//print(s.name+": IconX="+x+" IconY="+y+"  Size="+size+"\n");
        }
        public void move(float newX, float newY)
        {
            if (true)
            {
                //print(x);
            }

        }

    }
}
