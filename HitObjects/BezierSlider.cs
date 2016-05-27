using System;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class BezierSlider : Slider
    {
        private BezierCurve[] curves;

        public BezierSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "B")
                throw new ArgumentException("Error: Hitobject provided to BezierSlider class is not Bezier");
            this.GetCurves();
        }
        
        protected override int[] GetTickLocations(double tickinterval, int tickcount, int length)
        {
            List<Point> ticks = new List<Point>();

            //how many steps to travel through the curve
            //divide by curves.Length to scale this with the number of curves
            double steps = length*2 / curves.Length;
            //how much to increment t by with every loop
            double increment = 1 / steps;
            //how much along the curve we have traveled so far
            double travelled = 0;
            //where to get the next point on a given curve
            //assign  increment to get the next intended point
            double t = increment;
            Point prev = new Point();
            prev.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            prev.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));
            //which curve we are looking at
            int curvenumber = 0;
            while(curvenumber < curves.Length)
            {
                Point next = curves[curvenumber].Bezier(t);
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
                if(t > 1)
                {
                    curvenumber++;
                    t -= 1;
                }
            }

            if(travelled > 0)
                throw new Exception("Error, too many ticks to get in bezier curve, travelled=" + travelled);

            List<int> locations = new List<int>();
            foreach(Point i in ticks)
                locations.Add(i.IntX());

            return locations.ToArray();
        }

        protected override Point GetLastPoint(int length)
        {
            //how many steps to travel through the curve
            //divide by curves.Length to scale this with the number of curves
            double steps = length*2 / curves.Length;
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
            //which curve we are looking at
            int curvenumber = 0;
            while(curvenumber < curves.Length)
            {
                Point next = curves[curvenumber].Bezier(t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                travelled += distance;
                prev = next;
                if(travelled >= length)
                    return next;

                t += increment;
                if(t > 1)
                {
                    curvenumber++;
                    t -= 1;
                }
            }

            //If we reached the end of the slider without accumulated sliderlength distance,
            //just assume that the last point is the last point of the bezier curve
            return curves[curves.Length-1].Bezier(1);
        }

        //Uses the given list of control points to construct a list of bezier curves
        //to account for red points
        private void GetCurves()
        {
            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            List<Point> curvepoints = new List<Point>();
            List<BezierCurve> accumulatedcurves = new List<BezierCurve>();;

            curvepoints.Add(controlpoints[0]);
            for(int i = 1; i < controlpoints.Length; i++)
            {
                curvepoints.Add(controlpoints[i]);

                if(controlpoints[i].IntX() == controlpoints[i-1].IntX() &&
                    controlpoints[i].IntY() == controlpoints[i-1].IntY())
                {
                    BezierCurve tempcurve = new BezierCurve(initialcoord, curvepoints.ToArray());
                    accumulatedcurves.Add(tempcurve);

                    initialcoord = controlpoints[i];
                    curvepoints.Clear();
                }
            }

            if(curvepoints.Count > 0)
            {
                BezierCurve tempcurve = new BezierCurve(initialcoord, curvepoints.ToArray());
                accumulatedcurves.Add(tempcurve);
            }

            curves = accumulatedcurves.ToArray();
        }
    }
}
