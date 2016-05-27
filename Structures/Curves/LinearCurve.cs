using System;
using System.Collections.Generic;

namespace Structures.Curves
{
    public class LinearCurve
    {
        List<Point> points;
        
        //Constructs a Linear "curve" given a list of points
        public LinearCurve(Point startpoint, Point[] sliderpoints)
        {
            points = new List<Point>();
            points.Add(startpoint);
            points.AddRange(sliderpoints);
        }
        
        //Courtesy of http://math.stackexchange.com/questions/656500/given-a-point-slope-and-a-distance-along-that-slope-easily-find-a-second-p
        public Point GetPointAlong(double length)
        {
            Point begin = points[0];
            Point end = points[1];
            
            const double EPSILON = 1E-6;
            if (Math.Abs(end.x - begin.x) < EPSILON)
                return begin;

            double m = (end.y - begin.y) / (end.x - begin.x);
            
            Point along;

            if(end.x < begin.x)
                along.x = Math.Abs(length * (1 / Math.Sqrt(1 + Math.Pow(m, 2))) - begin.x);
            else
                along.x = length * (1 / Math.Sqrt(1 + Math.Pow(m, 2))) + begin.x;
            
            if(end.y < begin.y)
                along.y = Math.Abs(length * (m / Math.Sqrt(1 + Math.Pow(m, 2))) - begin.y);
            else
                along.y = length * (m / Math.Sqrt(1 + Math.Pow(m, 2))) + begin.y;
            
            return along;
        }
    }
}