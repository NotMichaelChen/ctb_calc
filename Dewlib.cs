using System;
using System.Collections.Generic;
using System.Linq;

using Structures;

// Collection of useful static functions
public static class Dewlib
{
    // Takes an array of strings and trims all of their trailing and leading whitespace
    public static string[] TrimStringArray(string[] arr)
    {
        for(int i = 0; i < arr.Length; i++)
        {
            arr[i] = arr[i].Trim();
        }
        return arr;
    }

    // Keeps the number in the range between lower and upper
    // If the bounds are exceeded, then the number will loop back around. This makes
    // it functionally different from clamp
    public static double RestrictRange(double num, double lower, double upper)
    {
        if(lower > upper)
            throw new ArgumentException("Error, lower is greater than upper\n" +
                                        "lower: " + lower + "\n" +
                                        "upper: " + upper);

        //A positive threshold is exceeded by going above it
        if(num > upper)
        {
            //Just in case the threshold is exceeded multiple times
            while(num > upper)
                num = num - upper + lower;
        }
        //A negative threshold is exceeded by going below it
        else if(num < lower)
        {
            while(num < lower)
                num = upper - (lower - num);
        }

        return num;
    }

    //Removes empty strings in a given array
    public static string[] RemoveEmptyEntries(string[] arr)
    {
        List<string> finalarr = new List<string>(arr);

        finalarr.RemoveAll(delegate(string str)
        {
            return str.Length == 0;
        });

        return finalarr.ToArray();
    }

    //Calculates the distance between two points
    public static double GetDistance(double a, double b, double x, double y)
    {
        return Math.Sqrt(Math.Pow(x - a, 2) + Math.Pow(y - b, 2));
    }
    
    //Calculates the nth row of pascal's triangle
    //The first row is n=0
    //Credit from http://stackoverflow.com/questions/15580291/how-to-efficiently-calculate-a-row-in-pascals-triangle
    public static int[] GetPascalRow(int n)
    {
        List<int> row = new List<int>();
        row.Add(1);
        for(int i = 0; i < n; i++)
        {
            row.Add((int)(row[i] * (n-i) / (double)(i+1)));
        }
        return row.ToArray();
    }
    
    //Sum a list of numbers with a given weight
    //The list is first sorted greatest to least, then each item is added after being
    //multiplied by weight^i
    public static double SumScaledList(double[] items, double weight)
    {   
        if(weight < 0 || weight > 1)
            throw new ArgumentOutOfRangeException("Error, weight is out of bounds\n" +
                                                  "weight=" + weight);
        Array.Sort(items);
        //to make the list greatest to least
        Array.Reverse(items);
        
        double sum = 0;
        for(int i = 0; i < items.Length; i++)
            sum += items[i] * Math.Pow(weight, i);
        
        return sum;
    }
    
    //Given a list of points, split the list along similar points
    //(eg if two points are the same and appear consecutively, then split the list there)
    public static Point[][] SplitPointList(Point[] pointlist)
    {
        List<List<Point>> splitlist = new List<List<Point>>();
        
        List<Point> accumulatedlist = new List<Point>();
        
        accumulatedlist.Add(pointlist[0]);
        for(int i = 1; i < pointlist.Length; i++)
        {
            if(pointlist[i].IntX() == pointlist[i-1].IntX() &&
               pointlist[i].IntY() == pointlist[i-1].IntY())
            {
                splitlist.Add(new List<Point>(accumulatedlist));
                accumulatedlist.Clear();
            }
            accumulatedlist.Add(pointlist[i]);
        }

        //Ignore redpoints at the end of sliders
        if(accumulatedlist.Count > 1)
        {
            splitlist.Add(accumulatedlist);
        }
        
        return splitlist.Select(a => a.ToArray()).ToArray();
    }
    
    //Split string only at the first occurance of char
    public static string[] SplitFirst(string s, char c)
    {
        int splitindex;
        for(splitindex = 0; splitindex < s.Length; splitindex++)
        {
            if(s[splitindex] == c)
            {
                string[] split = new string[2];
                //Don't want to include the splitting character, don't do splitindex+1
                split[0] = s.Substring(0, splitindex);
                //Ok to do +1 here, substring will just return an empty string if splitindex is the last valid char
                split[1] = s.Substring(splitindex+1);
                
                return split;
            }
        }
        
        //splitting character was not found, so just return the string given
        return new string[] {s};
    }
    
    //Removes/Replaces invalid characters in a potential filename
    public static string MakeValidFilename(string s)
    {
        //chars to outright remove
        string[] toremove = {"<", ">", "/", "\\", "|", "?", "*"};
        foreach (string invalid in toremove)
        {
            s = s.Replace(invalid, string.Empty);
        }
        
        //Replace : with ;
        s = s.Replace(":", ";");
        //Replace " with '
        s = s.Replace("\"", "\'");
        
        return s;
    }

    //Makes sure that val lies between max and min. If it’s greater than man, then it’s replaced by max, etc.
    //Different from RestrictRange as val does not loop around
    //Copied from http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if(val.CompareTo(max) > 0) return max;
        else return val;
    }
}
