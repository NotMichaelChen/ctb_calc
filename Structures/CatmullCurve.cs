using System;
using System.Collections.Generic;

namespace Structures
{
    //Represents a Catmull-Rom Spline given a list of control points
    public class CatmullCurve
    {
        private List<Point> controlpoints;

        public CatmullCurve(Point startpoint, Point[] sliderpoints)
        {
            controlpoints = new List<Point>();
            
            //Add the first point twice (so there are two points at the beginning)
            //and the last point twice (so there are two points at the end)
            //This is necessary for calculating the tangent variable for the curve
            controlpoints.Add(startpoint);
            controlpoints.Add(startpoint);
            controlpoints.AddRange(sliderpoints);
            controlpoints.Add(controlpoints[controlpoints.Count-1]);
        }
        
        public Point[] GetTickLocations(double interval, int count, int length)
        {
            List<Point> ticks = new List<Point>();

            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = length*6>1000?length*6:1000;
            //how much to increment t by with every loop
            double increment = 1 / steps;
            //how much along the curve we have traveled so far
            double travelled = 0;
            //where to get the next point on a given curve
            //assign increment to get the next intended point
            double t = increment;
            //track which curve (defined by two points) is being looked at
            //start at 1 to not break tangent points
            int curvestartpoint = 1;
            Point prev = controlpoints[0];
            //Subtract two for the extra points to get the number of curves
            while(curvestartpoint < controlpoints.Count - 2)
            {
                Point next = GetPointBetween(curvestartpoint, curvestartpoint+1, t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                travelled += distance;
                prev = next;
                if(travelled >= interval)
                {
                    ticks.Add(next);
                    travelled = 0;
                    if(ticks.Count == count)
                        break;
                }
                t += increment;
                if(t > 1)
                {
                    curvestartpoint++;
                    t -= 1;
                }
            }

            if(travelled > 0)
                throw new Exception("Error, too many ticks to get in catmull curve");

            return ticks.ToArray();
        }
        
        public Point GetPointAlong(int along)
        {
            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = along*5>1000?along*5:1000;
            //how much to increment t by with every loop
            double increment = 1 / steps;
            //how much along the curve we have traveled so far
            double length = 0;
            //where to get the next point on a given curve
            //assign increment to get the next intended point
            double t = increment;
            //track which curve (defined by two points) is being looked at
            //start at 1 to not break tangent points
            int curvestartpoint = 1;
            Point prev = controlpoints[0];
            //Subtract two for the extra points to get the number of curves
            while(curvestartpoint < controlpoints.Count - 2)
            {
                Point next = GetPointBetween(curvestartpoint, curvestartpoint+1, t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                length += distance;
                prev = next;
                if(length >= along)
                    return next;

                t += increment;
                if(t > 1)
                {
                    curvestartpoint++;
                    t -= 1;
                }
            }

            //If we reached the end of the slider without accumulated sliderlength distance,
            //just assume that the last point is the last point of the curve
            return GetPointBetween(controlpoints.Count - 2 - 1, controlpoints.Count - 2, 1);
        }

        //Accessed the point along the specified curve
        //start and end are the indexes of the points that the curve lays between
        //thus they cannot be 0 or controlpoints.Size-1
        //Courtesy of http://cubic.org/docs/hermite.htm
        private Point GetPointBetween(int start, int end, double t)
        {
            if(start > end)
                throw new ArgumentOutOfRangeException("Error: start is greater than end\n" +
                                                        "start: " + start + "\n" +
                                                        "end: " + end);

            double h1 = 2 * Math.Pow(t, 3) - 3 * Math.Pow(t, 2) + 1;
            double h2 = -2 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2);
            double h3 = Math.Pow(t, 3) - 2 * Math.Pow(t, 2) + t;
            double h4 = Math.Pow(t, 3) - Math.Pow(t, 2);

            Point T1, T2;
            T1.x = 0.5 * (controlpoints[end].x - controlpoints[start - 1].x);
            T1.y = 0.5 * (controlpoints[end].y - controlpoints[start - 1].y);
            T2.x = 0.5 * (controlpoints[end + 1].x - controlpoints[start].x);
            T2.y = 0.5 * (controlpoints[end + 1].y - controlpoints[start].x);

            Point result;
            result.x = h1 * controlpoints[start].x +
                        h2 * controlpoints[end].x +
                        h3 * T1.x +
                        h4 * T2.x;
            result.y = h1 * controlpoints[start].y +
                        h2 * controlpoints[end].y +
                        h3 * T1.y +
                        h4 * T2.y;

            return result;
        }
    }
}
