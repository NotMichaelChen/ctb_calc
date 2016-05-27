using System;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class PassthroughSlider : Slider
    {
        public PassthroughSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "P")
                throw new ArgumentException("Error: Hitobject provided to PassthroughSlider class is not Passthrough");
        }
        
        protected override int[] GetTickLocations(double tickinterval, int tickcount, int length)
        {
        }
        
        protected override Point GetLastPoint(int length)
        {
        }
    }
}
