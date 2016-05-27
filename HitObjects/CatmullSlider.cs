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
        public CatmullSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "C")
                throw new ArgumentException("Error: Hitobject provided to CatmullSlider class is not Catmull");
        }

        protected override int[] GetTickLocations(double tickinterval, int tickcount, int length)
        {
        }
        
        protected override Point GetLastPoint(int length)
        {
        }
    }
}
