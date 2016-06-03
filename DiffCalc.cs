using System;
using System.Linq;
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
    //Stores the position and time of each note in the beatmap
    private int[] positions;
    private int[] times;

    //Store the given beatmap and create a HitObjectParser from it
    public DiffCalc(Beatmap givenmap)
    {
        map = givenmap;

        //Checks that the beatmap given is the correct mode
        string mode = map.GetTag("general", "mode");
        if(!(mode == "0" || mode == "2"))
        {
            throw new Exception("Error: beatmap is not the correct mode (std or ctb)");
        }

        //Make a parser from the map
        hitobjects = new HitObjectListParser(map);
        
        this.GetPositionsAndTimes();
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
    
    /*public double GetDifficulty()
    {
        
    }*/

    //Calculates the average of the top ten % of speeds between hitpoints in the given beatmap
    //(speeds = change in position / change in time)
    public double CalculateDistances()
    {
        List<double> speeds = new List<double>();
        for(int i = 1; i < positions.Length; i++)
        {
            //Cast to make division operation a double
            speeds.Add(Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]));
        }

        return Dewlib.SumScaledList(speeds.ToArray(), 0.95);
    }
    
    public double GetDirectionalChanges()
    {
        double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
        CatcherInfo catcher = new CatcherInfo(circlesize);
        
        List<int> DCtimes = new List<int>();
        //Right is true, Left is false (make an enum later)
        bool currentdir = false;
        if(positions[0] - 256 > 0)
            currentdir = true;
        else
            currentdir = false;
        
        bool prevnotedir = currentdir;
        for(int i = 1; i < positions.Length; i++)
        {
            if(positions[i] == positions[i-1])
                continue;
            bool notedirection = false;
            if(positions[i] - positions[i-1] > 0)
                notedirection = true;
            else
                notedirection = false;
            
            int distance = Math.Abs(positions[i]-positions[i-1]);
            if(notedirection != currentdir && (distance > catcher.GetCatcherSize() || notedirection == prevnotedir))
            {
                currentdir = notedirection;
                DCtimes.Add(times[i]);
            }
            
            prevnotedir = currentdir;
        }
        
        //Directional Changes per section
        List<int> DCps = new List<int>();
        
        int threshold = 500;
        //count of DCs in a section
        int DCcounter = 0;
        int index = 0;
        while(index < DCtimes.Count)
        {
            if(DCtimes[index] > threshold)
            {
                DCps.Add(DCcounter);
                DCcounter = 0;
                threshold += 500;
            }
            else
            {
                DCcounter++;
                index++;
            }
        }
        //Account for leftover notes
        if(DCcounter > 0)
            DCps.Add(DCcounter);

        //Must convert int array to double array
        return Dewlib.SumScaledList(Array.ConvertAll(DCps.ToArray(), x => (double)x), 0.95);
    }
    
    public double GetJumpDifficulty()
    {
        //Calculating DC's, will be put into a method later
        double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
        CatcherInfo catcher = new CatcherInfo(circlesize);
        
        List<int> DCtimes = new List<int>();
        //Right is true, Left is false (make an enum later)
        bool currentdir = false;
        if(positions[0] - 256 > 0)
            currentdir = true;
        else
            currentdir = false;
        
        bool prevnotedir = currentdir;
        for(int i = 1; i < positions.Length; i++)
        {
            if(positions[i] == positions[i-1])
                continue;
            bool notedirection = false;
            if(positions[i] - positions[i-1] > 0)
                notedirection = true;
            else
                notedirection = false;
            
            int distance = Math.Abs(positions[i]-positions[i-1]);
            if(notedirection != currentdir && (distance > catcher.GetCatcherSize() || notedirection == prevnotedir))
            {
                currentdir = notedirection;
                DCtimes.Add(times[i]);
            }
            
            prevnotedir = currentdir;
        }
        
        //For debugging only, <difficulty, time>
        SortedList<double, int> notedifficulties = new SortedList<double, int>();

        for(int i = 1; i < positions.Length; i++)
        {
            //Cast to make division operation a double
            double velocity = Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]);
            //Temp value for hyperdashes
            if(velocity > 1)
            {
                velocity = 0.2;
                //velocity = 1.0 / (times[i] - times[i-1]);
            }
            else
            {
                //Scale normal jumps
                velocity = Math.Pow(velocity, 2);
            }
           
            //Implement smarter directional change multiplier later
            int DCindex = DCtimes.BinarySearch(times[i]);
            if(DCindex > 0)
            {
                int DCcount = 0;
                for(int j = DCindex; j >= 0 && DCtimes[DCindex] - DCtimes[j] < 1000; j--)
                    DCcount++;
                if(DCcount > 2)
                    velocity *= DCcount / 2;
            }
            //Scale velocity based on whether the previous note was a hyper dash or not, compared to this jump
            if(i > 1)
            {
                double prevvel = Math.Abs(positions[i-1] - positions[i-2]) / (double)(times[i-1] - times[i-2]);
                double thisvel = Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]);
                if(prevvel > 1 && thisvel <= 1)
                    velocity *= Math.Pow(thisvel, 1) * 4;
            }
            
            notedifficulties[velocity] = times[i];
        }
        
        return Dewlib.SumScaledList(notedifficulties.Keys.ToArray(), 0.95);
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
            else if(slidertype == "C")
                return new CatmullSlider(id, map);
            else return null;
        }
        else if(objecttype == HitObjectType.Spinner)
            return new Spinner();
        else
            throw new ArgumentException("Error: id is invalid");
    }
    
    //Gets the list of positions and times for each note of the beatmap, so that
    //other methods can use these lists without continuous calculations
    private void GetPositionsAndTimes()
    {
        List<int> positionslist = new List<int>();
        List<int> timeslist = new List<int>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            HitObjectWrapper hobject = this.GetHitObjectWrapper(hitobjects.GetHitObject(i));
            if(hobject == null)
                continue;

            try
            {
                positionslist.AddRange(hobject.GetHitLocations());
                timeslist.AddRange(hobject.GetHitTimes());
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\nobject=" + i);
            }
        }

        if(positionslist.Count != timeslist.Count)
            throw new Exception("Error: position and times array mismatched in size\n" +
                                "positions.Count: " + positionslist.Count + "\n" +
                                "times.Count: " + timeslist.Count);
        
        this.positions = positionslist.ToArray();
        this.times = timeslist.ToArray();
    }
}
