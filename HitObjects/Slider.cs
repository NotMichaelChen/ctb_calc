using System;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    //Represents a generic slider - as such it remains abstract
    abstract public class Slider : HitObjectWrapper
    {
        protected string id;
        protected Beatmap map;
        //List of control points of slider
        protected Point[] controlpoints;

        //Constructs a slider given an id
        //The beatmap given is the beatmap that the slider resides in
        //Used to make calculations related to timing
        public Slider(string tempid, Beatmap amap)
        {
            id = tempid;
            map = amap;

            //Checks that the hitobject given is actually a slider
            if(HitObjectParser.GetHitObjectType(id) != HitObjectType.Slider)
                throw new ArgumentException("Hitobject provided to slider class is not a slider");

            //Gets the control points of the slider in a formatted array of Points
            controlpoints = FormatControlPoints(id);
        }

        //Calculates the Milliseconds per Beat at a specified time by searching
        //through the entire timing points section
        //Timing points inside sliders don't affect the slider itself
        protected double GetMpB()
        {
            int ms = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //Get all the timing sections of the beatmap
            string[] timings = this.map.GetSection("TimingPoints");
            //Just in case there is only one timing point
            string timingpoint = timings[0];

            //Find the section that applies to the given time
            for(int i = 0; i < timings.Length; i++)
            {
                //Split the string by commas to get all the relevant times
                string[] attributes = timings[i].Split(new char[] {','});
                //Trim each string just in case
                attributes = Dewlib.TrimStringArray(attributes);

                if(Int32.Parse(attributes[0]) > ms)
                    break;

                else if(Double.Parse(attributes[1]) > 0)
                    timingpoint = timings[i];
                else
                    continue;
            }

            if(timingpoint == null)
                throw new Exception("Error, no relevant timing point\nms=" + ms);

            string[] properties = timingpoint.Split(new char[] {','});
            return Double.Parse(properties[1]);
        }

        //TODO: Throw error if somethings messed up with the timing section
        //Calculates the slider velocity at a specified time using the default
        //velocity and the relevant timing section
        //Timing points inside sliders don't affect the slider itself
        protected double GetSliderVelocity()
        {
            int ms = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //Get the default slider velocity of the beatmap
            double slidervelocity = Double.Parse(map.GetTag("Difficulty", "SliderMultiplier"));

            //Get all the timing sections of the beatmap
            string[] timings = this.map.GetSection("TimingPoints");
            //Will hold the relevant timing point
            string timingpoint = null;
            //Find the section that applies to the given time
            for(int i = 0; i < timings.Length; i++)
            {
                //Split the string by commas to get all the relevant times
                string[] attributes = timings[i].Split(new char[] {','});
                //Trim each string just in case
                attributes = Dewlib.TrimStringArray(attributes);
                //If the timing point is a higher time, then we want the previous timing section
                if(Int32.Parse(attributes[0]) > ms)
                {
                    //avoid accessing a negative timing point
                    if(i == 0)
                        timingpoint = timings[0];
                    else
                        timingpoint = timings[i - 1];
                    break;
                }
            }

            //If the timing point needed is the very last one
            if(timingpoint == null)
                timingpoint = timings[timings.Length-1];

            string[] properties = timingpoint.Split(new char[] {','});
            //If the offset is positive, then there is no slider multiplication
            if(Double.Parse(properties[1]) > 0)
                return slidervelocity;
            //Otherwise the slider multiplier is 100 / abs(offset)
            else
            {
                double offset = Double.Parse(properties[1]);
                return slidervelocity * (100 / Math.Abs(offset));
            }
        }

        //Remains abstract since implementation depends on the slider type
        abstract public int[] GetHitLocations();

        //Calculated the same regardless of slider type so can already be implemented
        public int[] GetHitTimes()
        {
            List<int> times = new List<int>();

            //When slider starts
            int starttime = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            double MpB = this.GetMpB();
            //How long the slider is in existance (without considering repeats)
            //slidertime = (pixellength / (slidervelocity * 100)) * MillisecondsPerBeat
            //(Order of operations is important kids! Otherwise you end up with slidertimes of 5000000 :o)
            int slidertime = Convert.ToInt32((Double.Parse(HitObjectParser.GetProperty(id, "pixellength")) / (this.GetSliderVelocity() * 100)) * MpB);
            //How long each tick is apart from each other
            //ticktime = MillisecondsPerBeat / tickrate
            int ticktime = Convert.ToInt32(MpB / Double.Parse(map.GetTag("difficulty", "slidertickrate")));
            //How many times the slider runs
            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            //How many ticks are in the slider (without repeats)
            //This is because later we use tickcount to tell how many times to add a time
            //for a given slider run
            int tickcount = this.GetTickCount() / sliderruns;

            //The time from the last tick to the slider end
            //If there are no ticks, then this just become slidertime
            int sliderenddiff = (slidertime) - (tickcount * ticktime);

            //Keeps track of what time we are at when travelling through the slider
            int currenttime = starttime;

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

        //Gets the number of slider ticks, including slider repeats
        //Calculated the same regardless of slider type
        private int GetTickCount()
        {
            int tickcount = 0;

            double slidervelocity = this.GetSliderVelocity();

            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
			int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));

			int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));

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

			return tickcount * sliderruns;
        }

        //Formats a string of control points into an array of points
        //Does NOT include the first hit point
		private Point[] FormatControlPoints(string id)
		{
            //Control point string will look like: B|380:120|332:96|332:96|304:124

            //Gets a list of strings containing each control point by splitting up the control point string
			string[] sliderpoints = HitObjectParser.GetProperty(id, "controlpoints").Split(new char[] {'|'});

			List<Point> temppoints = new List<Point>();

            //Parse each point as a Point object
			foreach(string point in sliderpoints)
			{
				string[] pair = point.Split(new char[] {':'});
				temppoints.Add(new Point(Double.Parse(pair[0]), Double.Parse(pair[1])));
			}

            //Return this list of points as an array
			return temppoints.ToArray();
		}
    }
}
