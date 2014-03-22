using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicGame;

namespace MusicGameTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void SetSampleType()
        {
            String type = "GuItAr";
            Sample s = new Sample("Assets/Sounds/GuitarG.wav", "GuitarThing", type);
            Assert.AreEqual(type, s.type);
        }
        [TestMethod]
        public void SetSampleName()
        {
            string name = "drums beat";
            Sample s = new Sample("Assets/Sounds/GuitarG.wav", name, "guitar");
            Assert.AreEqual(name, s.name);
        }
        [TestMethod]
        public void SetSampleSound()
        {
            String soundFile = "./GuitarG.wav";
            WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
            player.URL = soundFile;
            player.controls.play();
            Sample s = new Sample(soundFile, "Test", "guitar");
            Assert.AreEqual(player.currentMedia.name, s.player.currentMedia.name);
        }
        [TestMethod]
        public void AddSampleToTrack()
        {
            Track t = new Track(8, "guitar", 0);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            t.AddSample(3, s);
            Assert.AreEqual(s, t.samples[3]);
        }
        [TestMethod]
        public void AddWrongTypeSampleToTrack()
        {
            Track t = new Track(8, "guitar", 0);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "banjo");
            t.AddSample(3, s);
            Assert.AreNotEqual(s, t.samples[3]);
        }
        [TestMethod]
        public void CompareTracks()
        {
            Track t1 = new Track(8, "guitar", 0);
            Track t2 = new Track(8, "guitar", 4);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            t1.AddSample(3, s);
            t2.AddSample(3, s);
            Assert.IsTrue(CompareTracks(t1, t2));
        }
        [TestMethod]
        public void CompareTracksSamplesDifferentPositions()
        {
            Track t1 = new Track(8, "guitar", 0);
            Track t2 = new Track(8, "guitar", 4);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            t1.AddSample(3, s);
            t2.AddSample(5, s);
            Assert.IsFalse(CompareTracks(t1, t2));
        }
        [TestMethod]
        public void CompareTracksDifferentTypes()
        {
            Track t1 = new Track(8, "guitar", 0);
            Track t2 = new Track(8, "drums", 4);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");

            t1.AddSample(3, s);
            //Sample won't be added as it is a different type
            t2.AddSample(5, s);
            Assert.IsFalse(CompareTracks(t1, t2));
        }
        [TestMethod]
        public void CompareTracksDifferentLengths()
        {
            Track t1 = new Track(8, "guitar", 0);
            Track t2 = new Track(10, "guitar", 4);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            t1.AddSample(3, s);
            t2.AddSample(3, s);
            Assert.IsFalse(CompareTracks(t1, t2));
        }
        [TestMethod]
        public void MoveSample()
        {
            Track t1 = new Track(8, "guitar", 0);
            Sample s = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            t1.AddSample(5, s);
            t1.MoveSample(5, 2);
            Assert.AreEqual(s, t1.samples[2]);
        }
        [TestMethod]
        public void SwapSample()
        {
            Track t1 = new Track(8, "guitar", 0);
            Sample s1 = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            Sample s2 = new Sample("./GuitarD.wav", "guitarD", "guitar");
            t1.AddSample(5, s1);
            t1.AddSample(2, s2);
            t1.SwapSamples(5, 2);
            Assert.AreEqual(s1, t1.samples[2]);
        }
        [TestMethod]
        public void RemoveSample()
        {
            Track t1 = new Track(8, "guitar", 0);
            Sample s1 = new Sample("./GuitarG.wav", "guitarChord", "guitar");
            Sample s2 = new Sample("./GuitarD.wav", "guitarD", "guitar");
            t1.AddSample(5, s1);
            t1.RemoveSample(5);
            Assert.AreNotEqual(s1, t1.samples[5]);
        }
        
        public bool CompareTracks(Track t1, Track t2)
        {
            if (t1.trackLength == t2.trackLength)
            {
                for (int i = 0; i < t1.trackLength; i++)
                {
                    //If both slots are empty, ignore
                    if (t1.samples[i] == null && t2.samples[i] == null) { }
                    //If one is empty, not a match
                    else if ((t1.samples[i] != null && t2.samples[i] == null) || (t1.samples[i] == null && t2.samples[i] != null))
                    {
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
            else return false;
        }
    }
}
