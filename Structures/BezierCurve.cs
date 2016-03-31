using System;
using System.Collections.Generic;

namespace Structures
{
    //Represents a Bezier curve defined by a list of control points
    //Designed specifically to work with Sliders and their control points
    public class BezierCurve
    {
        List<Point> points;
        int sliderlength;

        //Constructs a Bezier curve given a list of points and a given length
        //This is necessary as a slider length may not be the actual length of the
        //defined curve
        public BezierCurve(Point startpoint, Point[] sliderpoints, double length)
        {
            points = new List<Point>();
            points.Add(startpoint);
            points.AddRange(sliderpoints);

            //length of the curve is NOT the length of the slider
            sliderlength = (int)Math.Round(length);

            ReassignLastPoint();
        }

        public Point[] GetPointInterval(double interval, int tickcount)
        {
            List<Point> ticks = new List<Point>();
            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = sliderlength*5>1000?sliderlength*5:1000;
            double length = 0;
            Point prev = points[0];
            for(int i = 1; i <= steps; i++)
            {
                double t = i / steps;
                Point next = Bezier(points, t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                prev = next;
                length += distance;
                if(length >= interval)
                {
                    ticks.Add(Bezier(points, t));
                    length = 0;
                    if(ticks.Count == tickcount)
                        break;
                }
            }

            if(length > 0)
            {
                ticks.Add(points[points.Count-1]);
            }
            Console.WriteLine(ticks.Count);
            return ticks.ToArray();
        }

        public Point GetPointAlong(double along)
        {
            if(along > sliderlength)
                throw new ArgumentException("Error: given length is beyond the length of the curve\n" +
                                            "length: " +  sliderlength + "\n" +
                                            "along: " + along);

            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = sliderlength*5>1000?sliderlength*5:1000;
            double length = 0;
            Point prev = points[0];
            for(int i = 1; i <= steps; i++)
            {
                double t = i / steps;
                Point next = Bezier(points, t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                prev = next;
                length += distance;
                if(length >= along)
                    return Bezier(points, t);
            }

            return points[points.Count-1];
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

        //Reassigns the last control point so that it coincides with the point that
        //is length units along the curve
        private void ReassignLastPoint()
        {
            //Make the number of steps either length * 5 or 1000, whichever is greater
            double steps = sliderlength*5>1000?sliderlength*5:1000;
            double length = 0;
            Point prev = points[0];
            for(int i = 1; i <= steps; i++)
            {
                double t = i / steps;
                Point next = Bezier(points, t);
                double distance = Dewlib.GetDistance(prev.x, prev.y, next.x, next.y);
                prev = next;
                length += distance;
                if(length >= sliderlength)
                {
                    prev.x = Math.Round(prev.x);
                    prev.y = Math.Round(prev.y);
                    points[points.Count-1] = prev;
                    return;
                }
            }

            throw new InvalidOperationException("Error: Bezier curve is shorter than given length\n" +
                                                "length: " + sliderlength + "\n" +
                                                "distance: " + length);
        }
    }
}
