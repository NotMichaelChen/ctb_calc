using System;
using System.Collections.Generic;

namespace Structures.Curves
{
    //Represents a Bezier curve defined by a list of control points
    public class BezierCurve
    {
        Point[] points;

        //Constructs a Bezier curve given a list of points
        public BezierCurve(Point[] slidercontrolpoints)
        {
            points = slidercontrolpoints;
        }
        
        //Calculates a point on the curve
        public Point Bezier(double t)
        {
            Point result = new Point(0,0);
            
            //Degree of the bezier curve
            int degree = points.Length-1;
            
            int[] pascalrow = Dewlib.GetPascalRow(degree);
            
            for(int i = 0; i < points.Length; i++)
            {
                result.x += pascalrow[i] * Math.Pow((1-t), degree-i) * Math.Pow(t, i) * points[i].x;
                result.y += pascalrow[i] * Math.Pow((1-t), degree-i) * Math.Pow(t, i) * points[i].y;
            }
            
            return result;
        }
    }
}
