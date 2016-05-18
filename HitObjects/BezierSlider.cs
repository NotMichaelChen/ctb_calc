using System;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    public class BezierSlider : Slider
    {
        private BezierCurve[] curves;

        public BezierSlider(string id, Beatmap amap) : base(id, amap)
        {
            if(HitObjectParser.GetProperty(id, "slidertype") != "B")
                throw new ArgumentException("Error: Hitobject provided to BezierSlider class is not Bezier");
        }

        public override int[] GetHitLocations()
        {
            List<int> hitpoints = new List<int>();
            List<int> ticklocs = new List<int>();

            //TODO: Add null checking to the statements using SearchForTag
            double slidervelocity = this.GetSliderVelocity();

            int tickrate = Int32.Parse(map.GetTag("Difficulty", "SliderTickRate"));
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"))));
            //Subtracting 1 returns the actual number of repeats
            int repeats = Int32.Parse(HitObjectParser.GetProperty(id, "repeat")) - 1;

            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            BezierCurve curve = new BezierCurve(initialcoord, controlpoints, length);

            //Get the first and last x-coordinates of the slider
            int beginpoint = Convert.ToInt32(initialcoord.x);
            int endpoint = Convert.ToInt32(curve.GetPointAlong(length).x);

            //If the slider is long enough to generate slider ticks
            //slidervelocity * (100/tickrate) == pixels between slider ticks
            if(length > slidervelocity * (100 / tickrate))
            {
                int tickcount = this.GetTickCount() / (repeats + 1);
                Point[] tickpoints = curve.GetPointInterval(slidervelocity * (100 / tickrate), tickcount);
                foreach(Point i in tickpoints)
                    ticklocs.Add((int)i.x);
                // /// Fill in all the ticks inside the slider
                // int ticklength = Convert.ToInt32(slidervelocity * (100 / tickrate));
                // //Will represent where the next tick is in the slider
                // int calclength = ticklength;
                // //While we haven't fallen off the end of the slider
                // while(calclength < length)
                // {
                //     ticklocs.Add(Convert.ToInt32(curve.GetPointAlong(calclength).x));
                //     //Move down the slider by a ticklength
                //     calclength += ticklength;
                // }
            }

            hitpoints.Add(beginpoint);
            hitpoints.AddRange(ticklocs);
            hitpoints.Add(endpoint);

            if(repeats > 0)
            {
                for(int i = 1; i <= repeats; i++)
                {
                    ticklocs.Reverse();
                    hitpoints.AddRange(ticklocs);
                    /// Add the endpoint or the beginpoint depending on whether
                    /// the slider is going forwards or backwards (repeat is even
                    /// or odd)
                    //even
                    if(i % 2 == 0)
                        hitpoints.Add(endpoint);
                    //odd
                    else
                        hitpoints.Add(beginpoint);
                }
            }

            //Return the hitpoints
            return hitpoints.ToArray();
        }

        //Uses the given list of control points to construct a list of bezier curves
        //to account for red points
        private void GetCurves()
        {
            //Get the initial hit point of the slider
            //Split into three lines for readibility
            Point initialcoord = new Point();
            initialcoord.x = Int32.Parse(HitObjectParser.GetProperty(id, "x"));
            initialcoord.y = Int32.Parse(HitObjectParser.GetProperty(id, "y"));

            List<Point> curvepoints = new List<Point>();
            List<BezierCurve> accumulatedcurves = new List<BezierCurve>();;

            curvepoints.Add(controlpoints[0]);
            for(int i = 1; i < controlpoints.Length; i++)
            {
                curvepoints.Add(controlpoints[i]);

                if(controlpoints[i].IntX() == controlpoints[i-1].IntX() &&
                    controlpoints[i].IntY() == controlpoints[i-1].IntY())
                {
                    BezierCurve tempcurve = new BezierCurve(initialcoord, curvepoints);
                    accumulatedcurves.Add(tempcurve);

                    initialcoord = controlpoints[i];
                    curvepoints.Clear();
                }
            }

            if(curvepoints.Count > 0)
            {
                BezierCurve tempcurve = new BezierCurve(initialcoord, curvepoints);
                accumulatedcurves.Add(tempcurve);
            }

            curves = accumulatedcurves.ToArray();
        }
    }
}
