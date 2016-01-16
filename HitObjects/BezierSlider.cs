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

        //Recursive definition of a bezier curve for any degree
        public static Point Bezier(List<Point> controls, double t)
        {
            if(controls.Count == 1)
                return controls[0];

            Point result = new Point();

            result.x = (1 - t) * Bezier(controls.GetRange(0, controls.Count - 1), t).x +
                        t * Bezier(controls.GetRange(1, controls.Count - 1), t).x;

            result.y = (1 - t) * Bezier(controls.GetRange(0, controls.Count - 1), t).y +
                        t * Bezier(controls.GetRange(1, controls.Count - 1), t).y;

            return result;
        }
    }
}
