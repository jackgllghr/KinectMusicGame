using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicGame
{
  class Track {
      private Boolean isPlaying, winState;
      public Sample[] samples;
      public int trackLength;
      private String type;
      float r, g, b;
      float x, y;
      float slotSize;

      public Track(int len, String trackType, float newX, float newY) {
        samples=new Sample[len];
        winState=false;
        isPlaying=true;
        trackLength=len;
        type=trackType;

        //size of slot
        slotSize=100;

        //Colour
        r=255;
        g=255;
        b=255;

        //set XY
        x=newX;
        y=newY;
      }
      //Add sample to track
      public void addSample(int i, Sample s) {
        //if (s.getType()==type) {
          //Assigns the sound s
          samples[i]=s;
          // Add xy coordinates to the sample[i]
          s.setXY(x+(slotSize*i), y);
          s.setSize(slotSize);
          s.setMoving(false);
        //}
        //else {
          //print("Wrong type of sound in "+type+" track");
        //}
      }
 
      //remove sound from track
      public void removeSample(int i) {
        samples[i]=null;
        //print("Removed slot "+i+"\n");
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
      public void setPlaying(bool b) {
          isPlaying = b;
      }
      public bool getPlaying() { return isPlaying; }
      /*void drawTrack(int time) {
        for (int i=0; i<len; i++) {
          if (i==time) {
            fill(200);
          }
          else {
            fill(r, g, b);
          }
          stroke(0);
          rect((i*slotSize)+x, y, slotSize, slotSize);
          //Draw line every 4 bars
          if (i%4==0) {
            stroke(255, 0, 0);
            line(x+(i*slotSize), y, x+(i*slotSize), y+slotSize);
          }
        }
      }*/
    
      public void drawIcons() {
        for (int i=0; i<samples.GetLength(1);i++) {
          //Draw icon if note is present
          if (samples[i]!=null) {
            //samples[i].drawIcon();
          }
        }
      }
      void setColor(float newR, float newG, float newB) {
        r=newR;
        g=newG;
        b=newB;
      }
      void win() {
        setColor(0, 255, 0);
        winState=true;
      }
  
      void drag(float clickX, float clickY) {
        for (int i=0; i<trackLength;i++) {
          if (samples[i]!=null && clickX>samples[i].x && clickX<samples[i].x+samples[i].size && clickY>samples[i].y && clickY<samples[i].y+samples[i].y+samples[i].size ) {
            samples[i].setMoving(true);
            //print(i);
          }
        }
      }
      /*void drop(float clickX, float clickY) {
        for (int i=0; i<trackLength;i++) {
          if (samples[i]!=null && samples[i].getMoving()) {  
       
            float v1=map(clickX, x, x+(slotSize*trackLength), 0, 15);
            int v=(int) v1;
            print(v);
            if (i==v) {
              break;
            }
            if (samples[v]==null) {
              addSample(v, samples[i]);
              removeSample(i);
              print("Moved from slot "+i+" to "+v+"\n");

              samples[v].setXY(v*slotSize+x, y);
              samples[v].isMoving=false;
            }
            else {
              print("Slot filled, choose another");
              samples[v].isMoving=false;
            }
          }
        }
      }*/
      void sendToTray(int i) {
        removeSample(i);
      }
    }
}
