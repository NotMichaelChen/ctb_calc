using System;
using System.Collections.Generic;

using Structures;
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

        public override double[] GetHitLocations()
        {
            throw new NotImplementedException();
        }

        public override double[] GetHitTimes()
        {
            throw new NotImplementedException();
        }
    }
}
