using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using BeatmapInfo;
using HitObjectInterpreter;
using Structures;
using HitObjects;

using Direction = CatcherInfo.Direction;

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
    
    public double GetJumpDifficulty()
    {
        double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
        CatcherInfo catcher = new CatcherInfo(circlesize);
        
        int[] DCtimes = GetDirectionalChangeTimes();
        
        List<HitPoint> notes = new List<HitPoint>();

        for(int i = 1; i < positions.Length; i++)
        {
            //Cast to make division operation a double
            double velocity = Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]);
            //Scale velocity to be a percent of a pixel-jump - a pixel jump is equal to 1
            velocity = catcher.PercentHyper(velocity);
            //Temp value for hyperdashes 
            if(velocity > 1)
            {
                //velocity = 0.2;
                velocity = 1.0 / (times[i] - times[i-1]);
            }
            else
            {
                //Scale normal jumps
                velocity = Math.Pow(velocity, 1);
            }
            
            //Multiply jump difficulty with CS
            velocity *= (circlesize + 1) / 2;
            
            double difficulty = velocity;
            
            //Distance is small enough to catch without moving
            if(Math.Abs(positions[i] - positions[i-1]) < catcher.GetCatcherSize())
            {
                var temp = Math.Abs(positions[i] - positions[i-1]);
                var temp2 = catcher.GetCatcherSize();
                //the distance the catcher moves, scaled as a percentage of how close the jump is to requiring movement
                double totalpercentdistance = 0;
                //index locating what note is the last note not needing movement to catch
                int nonmovementindex;
                //Makes sure that the notes aren't all in a straight line
                int leftmost = Math.Max(positions[i], positions[i-1]);
                int rightmost = Math.Min(positions[i], positions[i-1]);
                //used to make sure we only get at most 10 notes
                int nonmovecount = 0;
                int DCcount = 0;
                Direction curdir;
                if(positions[i] - positions[i-1] > 0)
                    curdir = Direction.Right;
                else if(positions[i] - positions[i-1] < 0)
                    curdir = Direction.Left;
                else
                    curdir = Direction.Stop;
                Direction prevdir = curdir;
                //while it's still a nonmovement jump
                for(nonmovementindex = i; nonmovementindex > 0 && nonmovecount <= 10; nonmovementindex--)
                {
                    if(positions[nonmovementindex] > leftmost)
                        leftmost = positions[nonmovementindex];
                    else if(positions[nonmovementindex] < rightmost)
                        rightmost = positions[nonmovementindex];
                    
                    if(positions[nonmovementindex] - positions[nonmovementindex-1] > 0)
                        curdir = Direction.Right;
                    else if(positions[nonmovementindex] - positions[nonmovementindex-1] < 0)
                        curdir = Direction.Left;
                    
                    if(leftmost - rightmost > catcher.GetCatcherSize())
                        break;
                    if(positions[nonmovementindex] - positions[nonmovementindex-1] > catcher.GetCatcherSize())
                        break;
                    
                    if(curdir != prevdir)
                        DCcount++;
                    
                    prevdir = curdir;
                    
                    totalpercentdistance += Math.Abs(positions[nonmovementindex] - positions[nonmovementindex-1]) / (double)catcher.GetCatcherSize();
                    nonmovecount++;
                }
                
                if(times[i] != times[nonmovementindex])
                    difficulty = 100 * ((double)DCcount / (times[i] - times[nonmovementindex])) * (Math.Pow(totalpercentdistance, 2.2) / 10);
                //if((totalpercentdistance / 10) * 12 > 1)
                    //difficulty = (totalpercentdistance / 10) * 12;
            }
           
            //Implement smarter directional change multiplier later
            int DCindex = Array.BinarySearch(DCtimes, times[i]);
            if(DCindex > 0)
            {
                int DCcount = 0;
                double DCsum = 0;
                for(int j = DCindex; j > 0 && DCcount <= 10; j--)
                {
                    DCcount++;
                    DCsum += DCtimes[j] - DCtimes[j-1];
                }
                
                //double DCmultiplier = DCcount / 3.0;
                //Want inverse of average, so flip sum and count
                double DCmultiplier = DCcount / DCsum * 1500;
                if(DCmultiplier > 1)
                    difficulty += velocity * DCmultiplier;
            }
            //Scale velocity based on whether the previous note was a hyper dash or not, compared to this jump
            if(i > 1 && DCindex > 0)
            {
                double prevvel = catcher.PercentHyper(Math.Abs(positions[i-1] - positions[i-2]) / (double)(times[i-1] - times[i-2]));
                double thisvel = catcher.PercentHyper(Math.Abs(positions[i] - positions[i-1]) / (double)(times[i] - times[i-1]));
                if(prevvel > 1 && thisvel <= 1)
                    difficulty += velocity * Math.Pow(thisvel, 1) * 1.1;
            }
            
            notes.Add(new HitPoint(positions[i], times[i], difficulty));
        }
        
        if(Program.IsDebug())
        {
            List<HitPoint> sortednotes = new List<HitPoint>(notes);
            sortednotes.Sort();
            
            Directory.CreateDirectory("debug");
            string filepath = map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version") + ".txt";
            filepath = "debug//" + filepath.Replace("/", "").Replace("\"", "\'");
            StreamWriter debugfile = new StreamWriter(filepath);
            foreach(HitPoint notepoint in sortednotes)
            {
                string leftvalue = notepoint.HitDifficulty.ToString();
                debugfile.WriteLine(leftvalue.PadRight(21) + notepoint.HitTime);
            }
            debugfile.Close();
        }
        
        List<double> notedifficulties = new List<double>();
        foreach(HitPoint notepoint in notes)
        {
            notedifficulties.Add(notepoint.HitDifficulty);
        }
        return Dewlib.SumScaledList(notedifficulties.ToArray(), 0.95);
    }
    
    //Each hitobject marked as a "DC" means that it requires a directional change to catch
    private int[] GetDirectionalChangeTimes()
    {
        double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
        CatcherInfo catcher = new CatcherInfo(circlesize);
        
        List<int> DCtimes = new List<int>();
        
        Direction prevnotedir = catcher.CurDirection;
        for(int i = 1; i < positions.Length; i++)
        {
            if(positions[i] == positions[i-1])
                continue;
            
            Direction notedirection;
            
            if(positions[i] - positions[i-1] > 0)
                notedirection = Direction.Right;
            else
                notedirection = Direction.Left;
            
            int distance = Math.Abs(positions[i]-positions[i-1]);
            if(notedirection != catcher.CurDirection && (distance > catcher.GetCatcherSize() || notedirection == prevnotedir))
            {
                catcher.CurDirection = notedirection;
                DCtimes.Add(times[i]);
            }
            
            prevnotedir = catcher.CurDirection;
        }
        
        return DCtimes.ToArray();
    }
    
    //Gets the list of positions and times for each note of the beatmap, so that
    //other methods can use these lists without continuous calculations
    private void GetPositionsAndTimes()
    {
        List<int> positionslist = new List<int>();
        List<int> timeslist = new List<int>();

        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            try
            {
                GenericHitObject hobject = new GenericHitObject(hitobjects.GetHitObjectID(i), map);
            
                positionslist.AddRange(hobject.GetHitLocations());
                timeslist.AddRange(hobject.GetHitTimes());
            }
            catch (Exception e)
            {
                //This is zero-indexed, so the first object is object=0
                throw new Exception(e.Message + "\nobject=" + i, e);
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
