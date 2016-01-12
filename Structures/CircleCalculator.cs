using System;

namespace Structures
{
    //Calculates a circle's radius and center given three points
    //Main algorithm courtesy of "Equation of a Circle from 3 Points
    //(2 dimensions)" from http://paulbourke.net/geometry/circlesphere/
    public class CircleCalculator
    {
        double radius;
        Point center;

        public CircleCalculator(Point p1, Point p2, Point p3)
        {
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
            center.y = (-1 / ma) * (center.x - ((p1.x + p2.x) / 2)) + ((p1.y + p2.y) / 2);

            radius = Math.Sqrt(Math.Pow(p1.x - center.x) + Math.Pow(p1.y - center.y));
        }

        public double Center
        {
            get { return center; }
        }

        public double Radius
        {
            get { return radius; }
        }
    }
}
