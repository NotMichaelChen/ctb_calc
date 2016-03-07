using System;

namespace Structures
{
    //Represents a Catmull-Rom Spline given a list of control points
    public class CatmullCurve
    {
        private List<Point> controlpoints;

        public CatmullCurve(List<Point> points)
        {
            controlpoints = points;
        }
    }
}
