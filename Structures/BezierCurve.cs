using System;
using System.Collections.Generic;

namespace Structures
{
    //Represents a Bezier curve defined by a list of control points
    //Designed specifically to work with Sliders and their control points
    public class BezierCurve
    {
        List<Point> points;

        //Constructs a Bezier curve given a list of points and a given length
        //This is necessary as a slider length may not be the actual length of the
        //defined curve
        public BezierCurve(Point startpoint, Point[] sliderpoints)
        {
            points = new List<Point>();
            points.Add(startpoint);
            points.AddRange(sliderpoints);
        }

        //Wrapper method to calculate a point on the curve
        public Point Bezier(double t)
        {
            return Bezier(points, t);
        }

        //Recursive definition of a bezier curve for any degree
        private Point Bezier(List<Point> controls, double t)
        {
            if(controls.Count == 1)
                return controls[0];

            Point result = new Point();

            result.x = (1 - t) * Bezier(controls.GetRange(0, controls.Count - 1), t).x +
                        t * Bezier(controls.GetRange(1, controls.Count - 1), t).x;

            result.y = (1 - t) * Bezier(controls.GetRange(0, controls.Count - 1), t).y +
                        t * Bezier(controls.GetRange(1, controls.Count - 1), t).y;

            return result;
        }
    }
}
