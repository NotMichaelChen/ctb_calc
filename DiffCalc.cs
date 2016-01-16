using System;
using System.Text;
using System.Collections.Generic;

using BeatmapInfo;
using HitObjectInterpreter;
using HitObjects;

// Takes a beatmap object and calculates a difficulty based off of it
public class DiffCalc
{
    //Holds the beatmap
    private Beatmap map;
    //Holds the hitobjects, wrapped in a HitObjectParser
    private HitObjectListParser hitobjects;

    //Store the given beatmap and create a HitObjectParser from it
    public DiffCalc(Beatmap givenmap)
    {
        map = givenmap;

        //Checks that the beatmap given is the correct mode
        string mode = map.GetTag("general", "mode");
        if(!(mode.Equals("0") || mode.Equals("2")))
        {
            throw new Exception("Error: beatmap is not the correct mode (std or ctb)");
        }

        //Make a parser from the map
        hitobjects = new HitObjectListParser(map);
    }

    //Gets a count of each type of hitobject in the beatmap
    //Returns an array of 3 ints, which are the counts of Circles, Sliders,
    //and Spinners respectively
    public int[] GetHitObjectsCount()
    {
        //0 = circles, 1 = sliders, 2 = spinners
        int[] counts = new int[3];

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            HitObjectType hobject = hitobjects.GetHitObjectType(i);
            if(hobject == HitObjectType.Circle)
                counts[0]++;
            else if(hobject == HitObjectType.Slider)
                counts[1]++;
            else if(hobject == HitObjectType.Spinner)
                counts[2]++;
        }
        return counts;
    }

    //Get a list of all hit points in a beatmap
    public int[] GetHitPoints()
    {
        List<int> positions = new List<int>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {

            HitObjectWrapper hobject = this.GetHitObjectWrapper(hitobjects.GetHitObject(i));
            if(hobject == null)
                continue;

            try
            {
                positions.AddRange(hobject.GetHitLocations());
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("index: " + i);
            }

        }

        return positions.ToArray();
    }

    //Get a list of all hit times in a beatmap
    public int[] GetHitTimes()
    {
        List<int> times = new List<int>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {

            HitObjectWrapper hobject = this.GetHitObjectWrapper(hitobjects.GetHitObject(i));
            if(hobject == null)
                continue;
            times.AddRange(hobject.GetHitTimes());

        }

        return times.ToArray();
    }

    //Calculates the average of the top ten % of speeds between hitpoints in the given beatmap
    //(speeds = change in position / change in time)
    public double CalculateDistances()
    {
        List<int> positions = new List<int>();
        List<int> times = new List<int>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            HitObjectWrapper hobject = this.GetHitObjectWrapper(hitobjects.GetHitObject(i));
            if(hobject == null)
                continue;

            positions.AddRange(hobject.GetHitLocations());
            times.AddRange(hobject.GetHitTimes());
        }

        if(positions.Count != times.Count)
            throw new Exception("Error: position and times array mismatched in size\n" +
                                "positions.Count: " + positions.Count + "\n" +
                                "times.Count: " + times.Count);

        List<double> speeds = new List<double>();
        for(int i = 1; i < positions.Count; i++)
        {
            //Cast to make division operation a double
            speeds.Add(Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]));
        }

        speeds.Sort();

        //Avoid dividing by zero if there aren't enough objects to make a top ten percent
        int topten;
        if(speeds.Count >= 10)
            topten = speeds.Count / 10;
        else
            topten = 1;

        double sum = 0;
        for(int i = speeds.Count - 1; i >= speeds.Count-topten; i--)
        {
            sum += speeds[i];
        }

        return sum/topten;
    }

    //Gets the hitobject returned as a HitObjectWrapper
    //Returns null if not implemented (hopefully won't exist when finished)
    //Throws an exception if the id is invalid
    private HitObjectWrapper GetHitObjectWrapper(string id)
    {
        HitObjectType objecttype = HitObjectParser.GetHitObjectType(id);
        if(objecttype == HitObjectType.Circle)
            return new Circle(id);
        else if(objecttype == HitObjectType.Slider)
        {
            string slidertype = HitObjectParser.GetProperty(id, "slidertype");
            if(slidertype == "L")
                return new LinearSlider(id, map);
            else if(slidertype == "P")
                return new PassthroughSlider(id, map);
            else if(slidertype == "B")
                return new BezierSlider(id, map);
            else return null;
        }
        else if(objecttype == HitObjectType.Spinner)
            return new Spinner();
        else
            throw new ArgumentException("Error: id is invalid");
    }
}
