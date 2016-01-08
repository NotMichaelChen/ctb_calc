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
}
