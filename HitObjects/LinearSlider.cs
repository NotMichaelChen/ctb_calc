using System;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class LinearSlider : Slider
    {
        //TODO: Make linearslider not require a beatmap
        public LinearSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "L")
                throw new ArgumentException("Error: Hitobject provided to LinearSlider class is not Linear");
        }

        //Courtesy of http://math.stackexchange.com/questions/656500/given-a-point-slope-and-a-distance-along-that-slope-easily-find-a-second-p
		private double GetEndLinear(Point begin, Point end, double d)
		{
			const double EPSILON = 1E-6;
			if (Math.Abs(end.x - begin.x) < EPSILON)
				return begin.x;

			double m = (end.y - begin.y) / (end.x - begin.x);

			if(end.x < begin.x)
				return Math.Abs(d * (1 / Math.Sqrt(1 + Math.Pow(m, 2))) - begin.x);
			else
				return d * (1 / Math.Sqrt(1 + Math.Pow(m, 2))) + begin.x;
		}

        public override double[] GetHitLocations()
        {
            /// Set up all variables needed for this method
            //Holds the x-coordinates of every hittable object in the slider
            List<double> hitpoints = new List<double>();
            //Holds the x-coordinate of every slider tick, without slider reversals
			List<double> ticklocs = new List<double>();
            //TODO: Add null checking to the statements using SearchForTag
            //Get the slider velocity of the beatmap
            double slidervelocity = Double.Parse(map.GetTag("Difficulty", "SliderMultiplier"));
            //Get the tickrate of the beatmap
            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //Gets the length of the slider in pixels
			double length = Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"));
            //Gets the number of times the slider repeats
			//Subtracting 1 returns the actual number of repeats
			int repeats = Int32.Parse(HitObjectParser.GetProperty(id, "repeat")) - 1;



            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Double.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Double.Parse(HitObjectParser.GetProperty(id, "y"));

            //Get the first and last x-coordinates of the slider
			double beginpoint = initialcoord.x;
			double endpoint = Convert.ToDouble(GetEndLinear(initialcoord, controlpoints[0], length));

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
					ticklocs.Add(Convert.ToInt32(GetEndLinear(initialcoord, controlpoints[0], calclength)));
                    //Move down the slider by a ticklength
					calclength += ticklength;
				}
			}

            //Add all the relevant points
			hitpoints.Add(beginpoint);
			hitpoints.AddRange(ticklocs);
			hitpoints.Add(endpoint);

            //If the slider has a repeat
			if(repeats > 0)
			{
                //For each repeat in the slider
				for(int i = 1; i <= repeats; i++)
				{
                    //Add the tick points in reverse
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
