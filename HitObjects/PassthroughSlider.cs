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
            List<double> hitpoints = new List<double>();
			List<double> ticklocs = new List<double>();

            //TODO: Add null checking to the statements using SearchForTag
            double slidervelocity = this.GetSliderVelocity();

            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //length can't be int since the circlecurve class requires a double for length
			double length = Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"));
			//Subtracting 1 returns the actual number of repeats
			int repeats = Int32.Parse(HitObjectParser.GetProperty(id, "repeat")) - 1;

            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Double.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Double.Parse(HitObjectParser.GetProperty(id, "y"));

            CircleCurve curve = new CircleCurve(initialcoord, controlpoints[0], controlpoints[1], length);

            //Get the first and last x-coordinates of the slider
			double beginpoint = initialcoord.x;
			double endpoint = curve.GetPoint(1).x;

            //If the slider is long enough to generate slider ticks
            //slidervelocity * (100/tickrate) == pixels between slider ticks
			if(length > slidervelocity * (100 / tickrate))
			{
                /// Fill in all the ticks inside the slider
				int ticklength = Convert.ToInt32(slidervelocity * (100 / tickrate));
                //Will represent where the next tick is in the slider
				int calclength = ticklength;
                //While we haven't fallen off the end of the slider
				while(calclength < length)
				{
                    //Using the angle created from the first hit point and the control point (since a linear
                    //slider ALWAYS has only one control points), and the length between ticks, calculate
                    //where the tick should land
                    ticklocs.Add(curve.GetPointAlong(calclength).x);
                    //Move down the slider by a ticklength
					calclength += ticklength;
				}
			}

			hitpoints.Add(beginpoint);
			hitpoints.AddRange(ticklocs);
			hitpoints.Add(endpoint);

			if(repeats > 0)
			{
				for(int i = 1; i <= repeats; i++)
				{
					ticklocs.Reverse();
					hitpoints.AddRange(ticklocs);
                    /// Add the endpoint or the beginpoint depending on whether
                    /// the slider is going forwards or backwards (repeat is even
                    /// or odd)
					//even
					if(i % 2 == 0)
						hitpoints.Add(endpoint);
					//odd
					else
						hitpoints.Add(beginpoint);
				}
			}

            //Return the hitpoints
			return hitpoints.ToArray();
        }

        public override double[] GetHitTimes()
        {
            throw new NotImplementedException();
        }
    }
}
