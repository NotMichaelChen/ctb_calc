using System;
using System.Linq;
using System.Collections.Generic;

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
        //No mode specified means standard (old maps have no mode)
        if(!(mode == "0" || mode == "2" || mode == null))
        {
            throw new Exception("Error: beatmap is not the correct mode (std or ctb)");
        }

        //Make a parser from the map
        hitobjects = new HitObjectListParser(map);
        
        this.GetPositionsAndTimes();
    }
    
    //Gets the title of the given beatmap, for display purposes
    //(Can be moved to Beatmap, but having it here is very convenient)
    public string GetBeatmapTitle()
    {
        return map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version");
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
    
    //Gets the difficulty of the entire map
    public double GetDifficulty()
    {
        HitPoint[] notes = this.GetNoteDifficulty();
        
        List<double> notedifficulties = new List<double>();
        foreach(HitPoint notepoint in notes)
        {
            notedifficulties.Add(notepoint.HitDifficulty);
        }
        return Dewlib.SumScaledList(notedifficulties.ToArray(), 0.95);
    }

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
    
    //Gets a list of every hitpoint and its associated difficulty
    public HitPoint[] GetNoteDifficulty()
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
                velocity = 0.2;
                //velocity = 1.0 / (times[i] - times[i-1]);
            }
            else
            {
                //Scale normal jumps
                velocity = Math.Pow(velocity, 2);
            }
            
            //Multiply jump difficulty with CS
            velocity *= Math.Pow(circlesize+1, 1.5) / 3;
            
            double difficulty = velocity;

            difficulty = this.CalculateNonmovementDifficulty(i, catcher, difficulty);
            
            difficulty = this.CalculateDCDensity(i, catcher, DCtimes, velocity, difficulty);
            
            difficulty = this.CalculateHyperChanges(i, DCtimes, catcher, difficulty);
            
            notes.Add(new HitPoint(positions[i], times[i], difficulty));
        }
        
        return notes.ToArray();
    }
    
    private double CalculateNonmovementDifficulty(int index, CatcherInfo catcher, double difficulty)
    {
        //Check if the last three notes can be caught without moving
        //Prevents unnecessary checking
        if(index > 2 && Math.Max(Math.Max(positions[index], positions[index-1]), positions[index-2]) - Math.Min(Math.Min(positions[index], positions[index-1]), positions[index-2]) < catcher.GetCatcherSize())
        {
            //the distance the catcher moves, scaled as a percentage of how close the jump is to requiring movement
            double totalpercentdistance = 0;
            //index locating what note is the last note not needing movement to catch
            int nonmovementindex;
            //Makes sure that the notes aren't all in a straight line
            int leftmost = Math.Max(positions[index], positions[index-1]);
            int rightmost = Math.Min(positions[index], positions[index-1]);
            //used to make sure we only get at most 10 notes
            int nonmovecount = 0;
            int DCcount = 0;
            Direction curdir;
            if(positions[index] - positions[index-1] > 0)
                curdir = Direction.Right;
            else if(positions[index] - positions[index-1] < 0)
                curdir = Direction.Left;
            else
                curdir = Direction.Stop;
            Direction prevdir = curdir;
            //while it's still a nonmovement jump
            for(nonmovementindex = index; nonmovementindex > 0 && nonmovecount <= 10; nonmovementindex--)
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
            
            if(times[index] != times[nonmovementindex])
            {
                //TODO: fix relation between time difference and DC count
                double pendingdiff = 100 * ((double)DCcount / (times[index] - times[nonmovementindex])) * (Math.Pow(totalpercentdistance, 3) / 100);
                difficulty = Math.Max(pendingdiff, difficulty);
            }
        }
        
        return difficulty;
    }
    
    private double CalculateDCDensity(int index, CatcherInfo catcher, int[] DCtimes, double basevelocity, double difficulty)
    {
        int DCindex = Array.BinarySearch(DCtimes, times[index]);
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
            double DCmultiplier = Math.Pow(DCcount / DCsum * 50, 3);
            difficulty += basevelocity * DCmultiplier;
            //Previous jump was a hyper
            if(index > 2 && times[index-1] != times[index-2] &&
               catcher.PercentHyper((positions[index-1] - positions[index-2]) / (times[index-1] - times[index-2])) > 1)
            {
                difficulty *= 2;
            }
        }
        
        return difficulty;
    }
    
    //Scale velocity based on whether the previous note was a hyper dash or not, compared to this jump
    private double CalculateHyperChanges(int index, int[] DCtimes, CatcherInfo catcher, double difficulty)
    {
        if(index > 1)
        {
            double prevvel = catcher.PercentHyper(Math.Abs(positions[index-1] - positions[index-2]) / (double)(times[index-1] - times[index-2]));
            double thisvel = catcher.PercentHyper(Math.Abs(positions[index] - positions[index-1]) / (double)(times[index] - times[index-1]));
            if(prevvel > 1 && thisvel <= 1)
            {
                difficulty *= 2;
                //next note requires a DC
                if(index + 1 < positions.Length && Array.BinarySearch(DCtimes, times[index+1]) > 0)
                    difficulty *= 2;
            }
        }
        
        return difficulty;
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
            
            bool isprevhyper;
            if(i > 1 && times[i-1] != times[i-2])
                isprevhyper = catcher.PercentHyper(Math.Abs(positions[i-1] - positions[i-2]) / (times[i-1] - times[i-2])) > 1;
            else
                isprevhyper = false;
            
            int distance = Math.Abs(positions[i]-positions[i-1]);
            double checkedsize = catcher.GetCatcherSize();
            if(isprevhyper)
                checkedsize /= 2;
            if(notedirection != catcher.CurDirection && (distance > checkedsize || notedirection == prevnotedir))
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
