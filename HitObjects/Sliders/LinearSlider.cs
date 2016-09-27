using System;
using System.Globalization;
using System.Collections.Generic;

using Structures;
using Structures.Curves;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects.Sliders
{
    public class LinearSlider : GenericSlider
    {
        LinearCurve[] curves;
        
        //Uses the given list of control points to construct a list of curves
        //to account for red points
        public LinearSlider(string id, Beatmap amap) : base(id, amap)
        {
            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            //List<Point> curvepoints = new List<Point>();
            List<LinearCurve> accumulatedcurves = new List<LinearCurve>();
            
            //Normal linear slider
            if(controlpoints.Length == 1)
            {
                accumulatedcurves.Add(new LinearCurve(initialcoord, controlpoints[0]));
            }
            else
            {
                List<Point> allcontrolpoints = new List<Point>();

                //Add first point only if it's not repeated in the control points (old maps)
                if(initialcoord.IntX() != controlpoints[0].IntX() && initialcoord.IntY() != controlpoints[0].IntY())
                    allcontrolpoints.Add(initialcoord);
                allcontrolpoints.AddRange(controlpoints);

                Point[][] curvepoints = Dewlib.SplitPointList(allcontrolpoints.ToArray());
                foreach(Point[] curve in curvepoints)
                {
                    if(curve.Length > 2)
                    {
                        for(int i = 1; i < curve.Length; i++)
                        {
                            accumulatedcurves.Add(new LinearCurve(curve[i-1], curve[i]));
                        }
                    }
                    else
                    {
                        accumulatedcurves.Add(new LinearCurve(curve[0], curve[1]));
                    }
                }
            }
            curves = accumulatedcurves.ToArray();
        }
        
        protected override int[] GetTickLocations()
        {
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);
            
            double slidervelocity = this.GetSliderVelocity();
            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"), CultureInfo.InvariantCulture);
            double ticklength = Math.Round(slidervelocity * (100 / tickrate), 4);
            
            List<int> ticks = new List<int>();
            
            if(length <= ticklength)
                return new int[0];
            
            //How far along a single curve we have traveled
            //Initialize to ticklength to make the while loop work for the first curve
            double accumulatedlength = ticklength;
            //How much along the entire slider we have traveled
            //Necessary to keep track of in case there are more curves than the slider length allows
            double totalaccumulatedlength = ticklength;
            //Special case for last curve, hence the curves.Length-1
            for(int i = 0; i < curves.Length; i++)
            {
                //Keep traveling down the curve accumulating ticks until we reach the length of the curve
                while(accumulatedlength < curves[i].DistanceBetween())
                {
                    ticks.Add(curves[i].GetPointAlong(accumulatedlength).IntX());
                    accumulatedlength += ticklength;
                    totalaccumulatedlength += ticklength;
                    //>= since ticks can't appear on slider ends
                    if(totalaccumulatedlength >= length)
                    {
                        //Don't want to bother with trying to break out of two loops
                        return ticks.ToArray();
                    }
                }
                accumulatedlength -= (int)Math.Round(curves[i].DistanceBetween());
            }
            
            return ticks.ToArray();
        }
        
        protected override Point GetLastPoint()
        {
            double length = Math.Round(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture), 4);
            //Only one curve
            if(curves.Length == 1)
            {
                return curves[0].GetPointAlong(length);
            }
            else
            {
                double accumulatedlength = 0;
                //Special behavior is needed for the last curve, hence the curves.Length-1
                for(int i = 0; i < curves.Length-1; i++)
                {
                    accumulatedlength += curves[i].DistanceBetween();
                }
                double lengthdifference = length - accumulatedlength;
                return curves[curves.Length-1].GetPointAlong(lengthdifference);
            }
        }
    }
}
