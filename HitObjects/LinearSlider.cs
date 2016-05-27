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
        }
        
        protected override Point GetLastPoint(int length)
        {
        }
    }
}
