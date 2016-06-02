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
        
        protected override int[] GetTickLocations()
        {
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            
            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            //Only need ticks for one slider length (no repeats needed)
            //Also no need for double conversion since TickCount is always divisible by sliderruns
            int tickcount = this.GetTickCount() / sliderruns;
            
            double slidervelocity = this.GetSliderVelocity();
            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            int ticklength = (int)Math.Round(slidervelocity * (100 / tickrate));
            
            List<int> ticks = new List<int>();
            
            if(length <= ticklength)
                return new int[0];
            
            //Will represent where the next tick is in the slider
            int calclength = ticklength;
            //While we haven't fallen off the end of the slider
            while(calclength < length)
            {
                ticks.Add(curve.GetPointAlong(calclength).IntX());
                //Move down the slider by a ticklength
                calclength += ticklength;
            }
            
            return ticks.ToArray();
        }
        
        protected override Point GetLastPoint()
        {
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            
            return curve.GetPointAlong(length);
        }
    }
}
