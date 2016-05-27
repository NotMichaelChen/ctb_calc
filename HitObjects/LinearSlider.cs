using System;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class LinearSlider : Slider
    {
        LinearCurve curve;
        
        public LinearSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "L")
                throw new ArgumentException("Error: Hitobject provided to LinearSlider class is not Linear");
            
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            
            curve = new LinearCurve(initialcoord, this.controlpoints);
        }
        
        protected override int[] GetTickLocations(double tickinterval, int tickcount, int length)
        {
            List<int> ticks = new List<int>();
            
            double slidervelocity = this.GetSliderVelocity();
            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            int ticklength = (int)Math.Round(slidervelocity * (100 / tickrate));
            
            //Will represent where the next tick is in the slider
            int calclength = ticklength;
            //While we haven't fallen off the end of the slider
            while(calclength < length)
            {
                ticks.Add(Convert.ToInt32(curve.GetPointAlong(calclength).x));
                //Move down the slider by a ticklength
                calclength += ticklength;
            }
            
            return ticks.ToArray();
        }
        
        protected override Point GetLastPoint(int length)
        {
            return curve.GetPointAlong(length);
        }
    }
}
