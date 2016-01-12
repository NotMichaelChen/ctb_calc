using System;

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

    /// Reverses the given string
    public static string ReverseString(string str)
    {
        if (str == null)
            return null;

       char[] array = str.ToCharArray();
       Array.Reverse(array);
       return new String(array);
    }

    // Adds two numbers, making sure to remain in the range between lower and upper
    public static double ModulusAdd(double num1, double num2, double lower, double upper)
    {
        if(lower > upper)
            throw new ArgumentException("Error, lower is greater than upper\n" +
                                        "lower: " + lower + "\n" +
                                        "upper: " + upper);

        double result = num1 + num2;

        //A positive threshold is exceeded by going above it
        if(result > upper)
        {
            //Just in case the threshold is exceeded multiple times
            while(result > upper)
                result = result - upper + lower;
        }
        //A negative threshold is exceeded by going below it
        else if(result < lower)
        {
            while(result < lower)
                result = upper - (lower - result);
        }

        return result;
    }
}
