using System;
using System.Collections.Generic;

namespace Structures.Curves
{
    //Represents a Bezier curve defined by a list of control points
    public class BezierCurve
    {
        //Not worth changing to an array
        List<Point> points;

        //Constructs a Bezier curve given a list of points
        public BezierCurve(Point startpoint, Point[] sliderpoints)
        {
            points = new List<Point>();
            points.Add(startpoint);
            points.AddRange(sliderpoints);
        }
        
        //Calculates a point on the curve
        public Point Bezier(double t)
        {
            Point result = new Point(0,0);
            
            //Degree of the bezier curve
            int degree = points.Count-1;
            
            int[] pascalrow = Dewlib.GetPascalRow(degree);
            
            for(int i = 0; i < points.Count; i++)
            {
                result.x += pascalrow[i] * Math.Pow((1-t), degree-i) * Math.Pow(t, i) * points[i].x;
                result.y += pascalrow[i] * Math.Pow((1-t), degree-i) * Math.Pow(t, i) * points[i].y;
            }
            
            return result;
        }
    }
}
