using System;
using System.Collections.Generic;

using Structures;
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

        public override int[] GetHitLocations()
        {
            throw new NotImplementedException();
        }
    }
}
