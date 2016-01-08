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

    public double[] GetHitPoints()
    {
        List<double> positions = new List<double>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {

            HitObjectWrapper hobject = this.GetHitObjectWrapper(hitobjects.GetHitObject(i));
            if(hobject == null)
                continue;
            positions.AddRange(hobject.GetHitLocations());

        }

        return positions.ToArray();
    }

    /*public double CalculateDistances()
    {
        List<double> positions = new List<double>();
        List<double> times = new List<double>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            HitObject hobject = hitobjects.GetHitObjectWrapper(i);
            positions.AddRange(hobject.GetHitLocations());
            times.AddRange(hobject.GetHitTimes());
        }

        List<double> speeds = new List<double>();
        //Add positions/times to speeds

		//foreach(double num in speeds)
		//	Console.WriteLine(num);
		speeds.Sort();

		int topten = speeds.Count / 10;

		double sum = 0;
		for(int i = speeds.Count - 1; i >= speeds.Count-topten; i--)
		{
			sum += speeds[i];
		}

		return sum/topten;
    }*/

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
            else return null;
        }
        else if(objecttype == HitObjectType.Spinner)
            return null;
        else
            throw new ArgumentException("Error: id is invalid");
    }
}
