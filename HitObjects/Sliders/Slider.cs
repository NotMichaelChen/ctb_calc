using System;
using System.Globalization;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects.Sliders
{
    //Represents a generic slider - as such it remains abstract
    abstract public class GenericSlider : HitObjectWrapper
    {
        protected string id;
        protected Beatmap map;
        //List of control points of slider
        //DOES NOT INCLUDE THE FIRST HIT POINT
        protected Point[] controlpoints;

        //Constructs a slider given an id
        //The beatmap given is the beatmap that the slider resides in
        //Used to make calculations related to timing
        protected GenericSlider(string tempid, Beatmap amap)
        {
            id = tempid;
            map = amap;

            //Checks that the hitobject given is actually a slider
            if(HitObjectParser.GetHitObjectType(id) != HitObjectType.Slider)
                throw new ArgumentException("Hitobject provided to slider class is not a slider");

            //Gets the control points of the slider in a formatted array of Points
            controlpoints = FormatControlPoints();
        }

        //Calculated the same regardless of slider type, but depends on GetTickLocations and GetLastPoint
        public int[] GetHitLocations()
        {
            List<int> hitpoints = new List<int>();
            List<int> ticklocs = new List<int>();

            //Subtracting 1 returns the actual number of repeats
            int repeats = Int32.Parse(HitObjectParser.GetProperty(id, "repeat")) - 1;

            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            //Get the first and last x-coordinates of the slider
            int beginpoint = initialcoord.IntX();
            ticklocs.AddRange(this.GetTickLocations());
            int endpoint = this.GetLastPoint().IntX();

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
            int slidertime = Convert.ToInt32((Double.Parse(HitObjectParser.GetProperty(id, "pixellength"), CultureInfo.InvariantCulture) / (this.GetSliderVelocity() * 100)) * MpB);
            //How long each tick is apart from each other
            //ticktime = MillisecondsPerBeat / tickrate
            int ticktime = Convert.ToInt32(MpB / Double.Parse(map.GetTag("difficulty", "slidertickrate"), CultureInfo.InvariantCulture));
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

        //Calculates the Milliseconds per Beat at a specified time by searching
        //through the entire timing points section
        //Timing points inside sliders don't affect the slider itself
        protected double GetMpB()
        {
            int ms = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //Get all the timing sections of the beatmap
            string[] timings = this.map.GetSection("TimingPoints");
            //Will hold the relevant timing point
            string timingpoint = null;
            //Store the index of the timing point just in case we need to iterate backwards later
            int index;
            //Find the section that applies to the given time
            for(index = 0; index < timings.Length; index++)
            {
                //Split the string by commas to get all the relevant times
                string[] attributes = timings[index].Split(',');
                //Trim each string just in case
                attributes = Dewlib.TrimStringArray(attributes);
                //If the timing point is a higher time, then we want the previous timing section
                if(Double.Parse(attributes[0]) > ms)
                {
                    //avoid accessing a negative timing point
                    if(index == 0)
                        timingpoint = timings[0];
                    else
                        timingpoint = timings[index - 1];
                    break;
                }
            }

            //If the timing point needed is the very last one
            if(timingpoint == null)
                timingpoint = timings[timings.Length-1];

            //If the mpb is negative, then we need to search backwards to find a positive one
            if(Double.Parse(timingpoint.Split(',')[1], CultureInfo.InvariantCulture) < 0)
            {
                for(int i = index-1; i >= 0; i--)
                {
                    //Split the string by commas to get all the relevant times
                    string[] attributes = timings[i].Split(',');
                    //Trim each string just in case
                    attributes = Dewlib.TrimStringArray(attributes);

                    if(Double.Parse(attributes[1]) > 0)
                    {
                        timingpoint = timings[i];
                        break;
                    }
                }
            }

            return Double.Parse(timingpoint.Split(',')[1], CultureInfo.InvariantCulture);
        }

        //TODO: Throw error if somethings messed up with the timing section
        //Calculates the slider velocity at a specified time using the default
        //velocity and the relevant timing section
        //Timing points inside sliders don't affect the slider itself
        protected double GetSliderVelocity()
        {
            int ms = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //Get the default slider velocity of the beatmap
            double slidervelocity = Double.Parse(map.GetTag("Difficulty", "SliderMultiplier"), CultureInfo.InvariantCulture);

            //Get all the timing sections of the beatmap
            string[] timings = this.map.GetSection("TimingPoints");
            //Will hold the relevant timing point
            string timingpoint = null;
            //Find the section that applies to the given time
            for(int i = 0; i < timings.Length; i++)
            {
                //Split the string by commas to get all the relevant times
                string[] attributes = timings[i].Split(',');
                //Trim each string just in case
                attributes = Dewlib.TrimStringArray(attributes);
                //If the timing point is a higher time, then we want the previous timing section
                if(Double.Parse(attributes[0]) > ms)
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

            string[] properties = timingpoint.Split(',');
            //If the offset is positive, then there is no slider multiplication
            if(Double.Parse(properties[1], CultureInfo.InvariantCulture) > 0)
                return slidervelocity;
            //Otherwise the slider multiplier is 100 / abs(offset)
            else
            {
                double offset = Double.Parse(properties[1], CultureInfo.InvariantCulture);
                return slidervelocity * (100 / Math.Abs(offset));
            }
        }

        //Gets the number of slider ticks, including slider repeats (but not slider ends)
        //Calculated the same regardless of slider type
        protected int GetTickCount()
        {
            double slidervelocity = this.GetSliderVelocity();

            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"), CultureInfo.InvariantCulture);
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture)));

            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            
            int ticklength = (int)Math.Round(slidervelocity * (100 / tickrate));
            
            int tickcount = length / ticklength;
            
            if(length % ticklength == 0)
                tickcount--;

            return tickcount * sliderruns;
        }
        
        //Get the x-coordinates of every tick in the slider
        abstract protected int[] GetTickLocations();
        //Get the last point of the slider, since it may not correspond with the last control point
        abstract protected Point GetLastPoint();

        //Formats a string of control points into an array of points
        //Does NOT include the first hit point
        private Point[] FormatControlPoints()
        {
            //Control point string will look like: B|380:120|332:96|332:96|304:124

            //Gets a list of strings containing each control point by splitting up the control point string
            string[] sliderpoints = HitObjectParser.GetProperty(id, "controlpoints").Split('|');

            List<Point> temppoints = new List<Point>();

            //Parse each point as a Point object
            foreach(string point in sliderpoints)
            {
                string[] pair = point.Split(':');
                temppoints.Add(new Point(Double.Parse(pair[0], CultureInfo.InvariantCulture), Double.Parse(pair[1], CultureInfo.InvariantCulture)));
            }

            //Return this list of points as an array
            return temppoints.ToArray();
        }
    }
}
