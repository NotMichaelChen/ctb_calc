using System;
using System.Collections.Generic;

using Structures;
using HitObjectInterpreter;
using BeatmapInfo;

namespace HitObjects
{
    //Represents a generic slider - as such it remains abstract
    abstract public class Slider : HitObjectWrapper
    {
        protected string id;
        protected Beatmap map;
        //List of control points of slider
        protected Point[] controlpoints;

        //Constructs a slider given an id
        //The beatmap given is the beatmap that the slider resides in
        //Used to make calculations related to timing
        public Slider(string tempid, Beatmap amap)
        {
            id = tempid;
            map = amap;

            //Checks that the hitobject given is actually a slider
            if(HitObjectParser.GetHitObjectType(id) != HitObjectType.Slider)
                throw new ArgumentException("Hitobject provided to slider class is not a slider");

            //Gets the control points of the slider in a formatted array of Points
            controlpoints = FormatControlPoints(id);
        }

        //Remains abstract since implementation depends on the slider type
        abstract public double[] GetHitLocations();
        abstract public double[] GetHitTimes();

        //Formats a string of control points into an array of points
        //Does NOT include the first hit point
		private Point[] FormatControlPoints(string id)
		{
            //Control point string will look like: B|380:120|332:96|332:96|304:124

            //Gets a list of strings containing each control point by splitting up the control point string
			string[] sliderpoints = HitObjectParser.GetProperty(id, "controlpoints").Split(new char[] {'|'});

			List<Point> temppoints = new List<Point>();

            //Parse each point as a Point object
			foreach(string point in sliderpoints)
			{
				string[] pair = point.Split(new char[] {':'});
				temppoints.Add(new Point(Double.Parse(pair[0]), Double.Parse(pair[1])));
			}

            //Return this list of points as an array
			return temppoints.ToArray();
		}
    }
}
