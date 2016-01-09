using System;
using System.Diagnostics;

using BeatmapInfo;

//TODO: Overhaul exception system
//TODO: Implement the rest of the sliders
//TODO: Replace .Equals() with strings with ==

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
            Stopwatch timer = new Stopwatch();
            try
            {
                foreach(string name in args)
                {
                    timer.Start();

                    Beatmap map = new Beatmap(name);
                    DiffCalc calc = new DiffCalc(map);

                    Console.WriteLine(map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version"));

                    //int[] hobjects = calc.GetHitObjectsCount();
                    //Console.WriteLine("Circles: " + hobjects[0] + " Sliders: " + hobjects[1] + " Spinners: " + hobjects[2]);

                    double[] hittimes = calc.GetHitPoints();
                    foreach(double i in hittimes)
                        Console.Write(i + " ");
                    Console.WriteLine();

                    timer.Stop();
                    Console.WriteLine("Calculation time (ms): " + timer.ElapsedMilliseconds);
                    timer.Reset();
                    Console.WriteLine();
                }

                Console.WriteLine("\nSuccess!");
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
