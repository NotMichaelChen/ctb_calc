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

        //Gets the number of slider ticks, including slider repeats
        private int GetTickCount()
        {
            int tickcount = 0;

            double slidervelocity = this.GetSliderVelocity();

            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
			int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
			//Subtracting 1 returns the actual number of repeats
			int repeats = Int32.Parse(HitObjectParser.GetProperty(id, "repeat")) - 1;

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
                    tickcount++;
					calclength += ticklength;
				}
			}

            //add one to repeats to avoid multiplying by zero
			return tickcount * (repeats + 1);
        }

        public override double[] GetHitLocations()
        {
            List<double> hitpoints = new List<double>();
			List<double> ticklocs = new List<double>();

            //TODO: Add null checking to the statements using SearchForTag
            double slidervelocity = this.GetSliderVelocity();

            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
			int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
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
            List<double> times = new List<double>();

            //When slider starts
            double starttime = Double.Parse(HitObjectParser.GetProperty(id, "time"));
            double MpB = this.GetMpB();
            //How long the slider is in existance (without considering repeats)
            //slidertime = (pixellength / (slidervelocity * 100)) * MillisecondsPerBeat
            //(Order of operations is important kids! Otherwise you end up with slidertimes of 5000000 :o)
            double slidertime = (Double.Parse(HitObjectParser.GetProperty(id, "pixellength")) / (this.GetSliderVelocity() * 100)) * MpB;
            //How long each tick is apart from each other
            //ticktime = MillisecondsPerBeat / tickrate
            double ticktime = MpB / Double.Parse(map.GetTag("difficulty", "slidertickrate"));
            //How many times the slider runs
            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            //How many ticks are in the slider (without repeats)
            //This is because later we use tickcount to tell how many times to add a time
            //for a given slider run
            int tickcount = this.GetTickCount() / sliderruns;

            //The time from the last tick to the slider end
            //If there are no ticks, then this just become slidertime
            double sliderenddiff = (slidertime) - (tickcount * ticktime);

            //Keeps track of what time we are at when travelling through the slider
            double currenttime = starttime;

            for(int runnum = 1; runnum <= sliderruns; runnum++)
			{
				if(runnum == 1)
				{
                    //Add the initial slider hit
					times.Add(currenttime);
                    //Add the tick times
					for(int ticknum = 0; ticknum < tickcount; ticknum++)
					{
						currenttime += ticktime;
						times.Add(currenttime);
					}
                    //Add the slider end
					currenttime += sliderenddiff;
					times.Add(currenttime);
				}
				else if(runnum % 2 == 0)
				{
                    //Add the first tick after the slider end
					currenttime += sliderenddiff;
					times.Add(currenttime);
                    //Don't skip the first tick since we need to include the slider head too
					for(int ticknum = 0; ticknum < tickcount; ticknum++)
					{
						currenttime += ticktime;
						times.Add(currenttime);
					}
				}
				else if(runnum % 2 == 1)
				{
                    //Add the tick times
					for(int ticknum = 0; ticknum < tickcount; ticknum++)
					{
						currenttime += ticktime;
						times.Add(currenttime);
					}
                    //Add the slider end
					currenttime += sliderenddiff;
					times.Add(currenttime);
				}
			}

            return times.ToArray();
        }
    }
}
