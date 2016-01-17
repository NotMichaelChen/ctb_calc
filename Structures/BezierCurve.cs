using System;
using System.Collections.Generic;

namespace Structures
{
    //Represents a Bezier curve defined by a list of control points
    //Designed specifically to work with Sliders and their control points
    public class BezierCurve
    {
        List<Point> points;
        int length;

        /* Current problem: Bezier curve gets defined by slider control points,
         * but the end of the slider is NOT the last control point. The slider
         * is shorter than the bezier curve that the control points define
         */
        public BezierCurve(Point startpoint, Point[] sliderpoints, double slidervelocity, int slidertime, doube mpb)
        {
            points = new List<Point>(startpoint);
            points.AddRange(sliderpoints);

            //length of the curve is NOT the length of the slider
            length = (slidervelocity * 100) * slidertime / mpb;
        }

        private Point GetPointAlong(double along)
        {
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            //Calculate how much along the curve the given length is
            double percent = along / length;

            if(percent > 1)
                throw new ArgumentException("Error: given length is beyond the length of the curve\n" +
                                            "length: " +  length + "\n" +
                                            "along: " + along);

            List<Point> pointlist = new List<Point>(this.controlpoints);

            return Bezier(pointlist, percent);
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
