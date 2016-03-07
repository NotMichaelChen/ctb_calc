using System;

namespace Structures
{
    //Represents a Catmull-Rom Spline given a list of control points
    public class CatmullCurve
    {
        private List<Point> controlpoints;
        private double length;

        public CatmullCurve(List<Point> points, double curvelength)
        {
            controlpoints = points;
            //Add the first point to the beginning (so there are two points at the beginning)
            //and the last point to the end (so there are two points at the end)
            //This is necessary for calculating the tangent variable for the curve
            controlpoints.Insert(0, controlpoints[0]);
            controlpoints.Add(controlpoints[controlpoints.Count-1]);

            length = curvelength;

            ReassignLastPoint();
        }

        //Get a point that is length units along the curve
        public Point GetPointAlong(double length)
        {

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
        private Point GetPointBetween(int start, int end, double t)
        {

        }

        //Reassigns the last control point so that it coincides with the point that
        //is length units along the curve
        private void ReassignLastPoint()
        {

        }
    }
}
