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
            throw new NotImplementedException();
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
    }
}
