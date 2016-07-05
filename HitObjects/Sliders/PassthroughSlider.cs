using System;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects.Sliders
{
    public class PassthroughSlider : GenericSlider
    {
        CircleCurve curve;
        
        public PassthroughSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(controlpoints.Length != 2)
                throw new ArgumentException("Error: Passthrough slider does not have 2 control points\n" +
                                            "controlpoints.Length=" + controlpoints.Length);
            
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            
            curve = new CircleCurve(initialcoord, controlpoints[0], controlpoints[1], length);
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
            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            int ticklength = (int)Math.Round(slidervelocity * (100 / tickrate));
            
            if(length <= ticklength)
                return new int[0];
            
            List<int> ticks = new List<int>();
            
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
