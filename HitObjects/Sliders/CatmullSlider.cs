using System;
using System.Globalization;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects.Sliders
{
    public class CatmullSlider : GenericSlider
    {
        CatmullCurve curve;
        
        public CatmullSlider(string id, Beatmap amap) : base(id, amap)
        {
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            
            curve = new CatmullCurve(initialcoord, this.controlpoints);
        }

        protected override int[] GetTickLocations()
        {
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);
            
            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            //Only need ticks for one slider length (no repeats needed)
            //Also no need for double conversion since TickCount is always divisible by sliderruns
            int tickcount = this.GetTickCount() / sliderruns;
            
            double slidervelocity = this.GetSliderVelocity();
            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"), CultureInfo.InvariantCulture);
            double ticklength = Math.Round(slidervelocity * (100 / tickrate), 4);
            
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
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);
            
            return curve.GetPointAlong(length);
        }
    }
}
