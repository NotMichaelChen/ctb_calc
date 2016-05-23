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

        //Gets a point along the curve
        //t is the parameter variable that controls where along the curve the point is
        //t goes from 0 to 1
        public Point GetPoint(double t)
        {
            //TODO: Make exception more useful
            if(t < 0 || t > 1)
                throw new ArgumentOutOfRangeException();

            //Subtract two for the extra points, and subtract one to get the number of curves
            //that the overall curve is composed of
            int curvecount = controlpoints.Count - 2 - 1;

            //Determines which curve the point specified by t lands on
            int curvetoget = 0;
            for(double i = 1; i <= curvecount; i++)
            {
                if(i / curvecount > t)
                {
                    curvetoget = (int)i;
                    break;
                }
            }

            //Scale the t so that it fits with the curve being accessed
            //Determines from what point the t is from
            //eg if t is 0.75 and there are 3 control points, then lowerbound is 0.5
            double lowerbound = (curvetoget - 1) / curvecount;
            //Determines how much along that specific curve the t is
            //eg fromlowerbound would be 0.25
            double fromlowerbound = t - lowerbound;
            //Divide the fromlowerbound from the range that the specified curve takes up from 0 to 1
            //eg the fraction would be 0.25 / 0.5 = 0.5
            double scaledt = fromlowerbound / ((curvetoget / curvecount) - lowerbound);

            //Get the two points that define the specified curve
            int startindex = curvetoget + 1;
            int endindex = startindex + 1;

            //Access the relevant point on that curve using the scaled t
            return GetPointBetween(startindex, endindex, scaledt);
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
