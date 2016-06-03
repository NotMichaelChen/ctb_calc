using System;
using System.Diagnostics;
using System.Collections.Generic;

using BeatmapInfo;

//TODO: Overhaul exception system

public class Program
{
    public static void Main(string[] args)
    {
        //Program should only work when a .osu file is dragged onto the program
        //Display a message if that's not done
        if(args.Length == 0)
        {
            Console.WriteLine("CTB Difficulty Analyzer");
            Console.WriteLine("Just drag your beatmap (.osu file) onto this program to measure difficulty");
            Console.WriteLine("Press any key to exit...");
        }
        //Otherwise try to run the program, and catch and display any exceptions that arise
        else
        {
            Console.WriteLine("Calculating...");
            SortedList<double, string> beatmaps = new SortedList<double, string>();
            Stopwatch timer = new Stopwatch();
            try
            {
                int count = 0;
                foreach(string name in args)
                {
                    timer.Start();

                    Beatmap map = new Beatmap(name);
                    DiffCalc calc = new DiffCalc(map);
                    
                    string title = map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version") + ": \t";
                    double difficulty = calc.GetJumpDifficulty();

                    timer.Stop();
                    
                    title += timer.ElapsedMilliseconds;
                    beatmaps[difficulty] = title;
                    
                    timer.Reset();
                    count++;
                    Console.Write(Math.Round((double)count * 100 / args.Length) + "%\r");
                }
                
                Console.WriteLine("\n");
                for(int i = beatmaps.Count - 1; i >= 0; i--)
                {
                    string[] titleandtime = beatmaps.Values[i].Split(new char[] {'\t'});
                    Console.WriteLine(titleandtime[0] + beatmaps.Keys[i]);
                    Console.WriteLine("Calculation Time (ms): " + titleandtime[1] + "\n");
                }

                Console.WriteLine("\nDone.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //Just in case the timer is still running
                timer.Stop();
            }
        }

        Console.ReadKey();
    }
}
