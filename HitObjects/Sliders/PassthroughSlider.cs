using System;
using System.Globalization;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects.Sliders
{
    public class PassthroughSlider : GenericSlider
    {
        CircleCurve curve;

        public PassthroughSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(controlpoints.Length != 2)
                throw new ArgumentException("Error: Passthrough slider does not have 2 control points\n" +
                                            "controlpoints.Length=" + controlpoints.Length);

            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);

            curve = new CircleCurve(initialcoord, controlpoints[0], controlpoints[1], length);
        }

        protected override int[] GetTickLocations()
        {
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);

            double slidervelocity = this.GetSliderVelocity();
            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"), CultureInfo.InvariantCulture);
            double ticklength = Math.Round(slidervelocity * (100 / tickrate), 4);

            if(length <= ticklength)
                return new int[0];

            List<int> ticks = new List<int>();

            //Will represent where the next tick is in the slider
            double calclength = ticklength;
            //While we haven't fallen off the end of the slider
            while(calclength < length)
            {
                ticks.Add(curve.GetPointAlong(calclength).IntX());
                //Move down the slider by a ticklength
                calclength += ticklength;
            }

            return ticks.ToArray();
        }

        protected override Point GetLastPoint()
        {
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);

            return curve.GetPointAlong(length);
        }
    }
}
