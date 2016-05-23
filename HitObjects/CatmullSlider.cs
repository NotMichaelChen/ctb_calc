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
            List<int> hitpoints = new List<int>();
            List<int> ticklocs = new List<int>();

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
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            
            CatmullCurve curve = new CatmullCurve(initialcoord, this.controlpoints);

            //Get the first and last x-coordinates of the slider
            int beginpoint = initialcoord.IntX();
            int endpoint = this.GetLastPoint(curve, length).IntX();

            int ticklength = (int)Math.Round(slidervelocity * (100 / tickrate));
            //If the slider is long enough to generate slider ticks
            //slidervelocity * (100/tickrate) == pixels between slider ticks
            if(length > ticklength)
            {
                //Only need ticks for one slider length (no repeats needed)
                int tickcount = this.GetTickCount() / (repeats+1);
                ticklocs.AddRange(this.GetTickLocations(curve, ticklength, tickcount, length));
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
        
        private int[] GetTickLocations(CatmullCurve curve, double tickinterval, int tickcount, int sliderlength)
        {
            List<Point> ticks = new List<Point>();

            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = sliderlength*5>1000?sliderlength*5:1000;
            //how much to increment t by with every loop
            double increment = 1 / steps;
            //how much along the curve we have traveled so far
            double travelled = 0;
            //where to get the next point on a given curve
            //assign increment to get the next intended point
            double t = increment;
            Point prev = new Point();
            prev.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            prev.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            while(t < 1)
            {
                Point next = curve.GetPoint(t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                travelled += distance;
                prev = next;
                if(travelled >= tickinterval)
                {
                    ticks.Add(next);
                    travelled = 0;
                    if(ticks.Count == tickcount)
                        break;
                }
                t += increment;
            }

            if(travelled > 0)
                throw new Exception("Error, too many ticks to get in catmull curve");

            List<int> locations = new List<int>();
            foreach(Point i in ticks)
                locations.Add(i.IntX());

            return locations.ToArray();
        }
        
        private Point GetLastPoint(CatmullCurve curve, int sliderlength)
        {
            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = sliderlength*5>1000?sliderlength*5:1000;
            //how much to increment t by with every loop
            double increment = 1 / steps;
            //how much along the curve we have traveled so far
            double length = 0;
            //where to get the next point on a given curve
            //assign increment to get the next intended point
            double t = increment;
            Point prev = new Point();
            prev.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            prev.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            while(t < 1)
            {
                Point next = curve.GetPoint(t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                length += distance;
                prev = next;
                if(length >= sliderlength)
                    return next;

                t += increment;
            }

            //If we reached the end of the slider without accumulated sliderlength distance,
            //just assume that the last point is the last point of the curve
            return curve.GetPoint(1);
        }
    }
}
