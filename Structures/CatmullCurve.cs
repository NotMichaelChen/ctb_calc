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

        //Gets a point along the curve
        //t is the parameter variable that controls where along the curve the point is
        //t goes from 0 to 1
        public Point GetPoint(double t)
        {

        }

        //Get a point that is length units along the curve
        public Point GetPointAlong(double length)
        {

        }

        //Reassigns the last control point so that it coincides with the point that
        //is length units along the curve
        private void ReassignLastPoint()
        {

        }
    }
}
