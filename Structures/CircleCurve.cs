using System;

using HitObjectInterpreter;

namespace Structures
{
    //Approximates a curve of three points using a circle calculated from those
    //three points.
    //Main algorithm courtesy of "Equation of a Circle from 3 Points
    //(2 dimensions)" from http://paulbourke.net/geometry/circlesphere/
    public class CircleCurve
    {
        double radius;
        Point center;

        double startangle, endangle;

        public CircleCurve(Point p1, Point p2, Point p3, double arclength)
        {
            //Assign this before points are shuffled around
            Point startpoint = p1;

            if(p1.x == p2.x)
            {
                Point temppoint = p2;
                p2 = p3;
                p3 = temppoint;
            }
            else if(p2.x == p3.x)
            {
                Point temppoint = p1;
                p1 = p2;
                p2 = temppoint;
            }

            double ma = (p2.y - p1.y) / (p2.x - p1.x);
            double mb = (p3.y - p2.y) / (p3.x - p2.x);

            center = new Point();

            center.x = (ma * mb * (p1.y - p3.y) + mb * (p1.x + p2.x) - ma * (p2.x + p3.x)) /
                            (2 * (mb - ma));

            if(ma == 0)
                center.y = (-1 / mb) * (center.x - ((p2.x + p3.x) / 2)) + ((p2.y + p3.y) / 2);
            else
                center.y = (-1 / ma) * (center.x - ((p1.x + p2.x) / 2)) + ((p1.y + p2.y) / 2);

            radius = Math.Sqrt(Math.Pow(p1.x - center.x, 2) + Math.Pow(p1.y - center.y, 2));

            this.startangle = Math.Acos((startpoint.x - center.x) / radius);

            //Use the sliderlength to calculate the final angle since the last control point
            //of the slider is NOT the last hit point of the slider
            //This is an angle differential since the arclength is the slider length, and the
            //formula assumes a start from an angle of 0
            double anglediff = arclength / radius;

            this.endangle = startangle + anglediff;
        }

        public Point Center
        {
            get { return center; }
        }

        public double Radius
        {
            get { return radius; }
        }

        //Gets a point on the approximated curve on the circle
        //Goes from 0 to 1, where 0 is the starting point and 1 is the ending point
        public Point GetPoint(double t)
        {
            //TODO: Make exception more useful
            if(t < 0 || t > 1)
                throw new ArgumentOutOfRangeException();

            double angle = t * (endangle - startangle) + startangle;

            Point accessed = new Point();
            accessed.x = center.x + radius * Math.Cos(angle);
            accessed.y = center.y + radius * Math.Sin(angle);

            return accessed;
        }
    }
}