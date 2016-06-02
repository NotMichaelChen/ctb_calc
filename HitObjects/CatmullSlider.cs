using System;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class CatmullSlider : Slider
    {
        CatmullCurve curve;
        
        public CatmullSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "C")
                throw new ArgumentException("Error: Hitobject provided to CatmullSlider class is not Catmull");
            
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            
            curve = new CatmullCurve(initialcoord, this.controlpoints);
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
            
            if(length <= ticklength)
                return new int[0];
            
            Point[] ticklocs = curve.GetTickLocations(ticklength, tickcount, length);
            
            List<int> xcoords = new List<int>();
            
            foreach(Point i in ticklocs)
                xcoords.Add(i.IntX());
            
            return xcoords.ToArray();
        }
        
        protected override Point GetLastPoint()
        {
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            
            return curve.GetPointAlong(length);
        }
    }
}
