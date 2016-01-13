using System;

using HitObjectInterpreter;

namespace HitObjects
{
    //Represents a circle
    public class Circle : HitObjectWrapper
    {
        string circleid;

        public Circle(string id)
        {
            circleid = id;
        }

        public int[] GetHitLocations()
        {
            //There is only one hit location for a circle, so just return an array
            //that holds that hit location
            string loc = HitObjectParser.GetProperty(circleid, "X");

            return new int[1] {Convert.ToInt32(loc)};
        }

        public int[] GetHitTimes()
        {
            //There is only one hit time for a circle, so just return an array
            //that holds that hit time
            string time = HitObjectParser.GetProperty(circleid, "time");

            return new int[1] {Convert.ToInt32(time)};
        }
    }
}
