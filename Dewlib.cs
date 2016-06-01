using System;
using System.Collections.Generic;

// Collection of useful static functions
public static class Dewlib
{
    /// Takes an array of strings and trims all of their trailing and leading whitespace
    public static string[] TrimStringArray(string[] arr)
    {
        for(int i = 0; i < arr.Length; i++)
        {
            arr[i] = arr[i].Trim();
        }
        return arr;
    }

    // Keeps the number in the range between lower and upper
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

        finalarr.RemoveAll(isEmpty);

        return finalarr.ToArray();
    }

    //Predicate method for RemoveEmptyEntries that determines if a string is empty
    private static bool isEmpty(string str)
    {
        return str.Length == 0;
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
    
    //Gets the top percentage of an array of doubles
    public static double GetPercentage(double[] items, double percentage)
    {
        if(percentage < 0 || percentage > 1)
            throw new ArgumentOutOfRangeException("Error, percentage is out of bounds\n" +
                                                  "percentage=" + percentage);
        
        Array.Sort(items);

        //Avoid dividing by zero if there aren't enough objects to make a top percentage
        int toppercentcount;
        if(items.Length >= percentage * 100)
            toppercentcount = items.Length / 10;
        else
            toppercentcount = 1;

        double sum = 0;
        for(int i = items.Length - 1; i >= items.Length-toppercentcount; i--)
        {
            sum += items[i];
        }

        return sum/toppercentcount;
    }
}
