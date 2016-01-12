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

    // Adds two numbers and rolls back to reset if the threshold is passed
    public static double ModulusAdd(double num1, double num2, double threshold, double reset = 0)
    {
        double result = num1 + num2;
        //A positive threshold is exceeded by going above it
        if(threshold > 0 && result > threshold)
        {
            //Just in case the threshold is exceeded multiple times
            while(result > threshold)
                result = result - threshold + reset;
        }
        //A negative threshold is exceed by going below it
        else if(threshold < 0 && result < threshold)
        {
            while(result < threshold)
                result = threshold - result + reset;
        }

        return result;
    }
}
