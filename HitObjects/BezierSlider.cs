using System;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class BezierSlider : Slider
    {
        public BezierSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "B")
                throw new ArgumentException("Error: Hitobject provided to BezierSlider class is not Bezier");
        }

        public override int[] GetHitLocations()
        {
            throw new NotImplementedException();
        }
    }
}
