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

        protected override int[] GetTickLocations(double tickinterval, int tickcount, int length)
        {
            Point[] ticklocs = curve.GetTickLocations(tickinterval, tickcount, length);
            
            List<int> xcoords = new List<int>();
            
            foreach(Point i in ticklocs)
                xcoords.Add(i.IntX());
            
            return xcoords.ToArray();
        }
        
        protected override Point GetLastPoint(int length)
        {
            return curve.GetPointAlong(length);
        }
    }
}
