using System;
using System.Diagnostics;

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
            Stopwatch timer = new Stopwatch();
            try
            {
                foreach(string name in args)
                {
                    timer.Start();

                    Beatmap map = new Beatmap(name);
                    DiffCalc calc = new DiffCalc(map);

                    Console.Write(map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version"));
                    Console.WriteLine(": " + calc.GetJumpDifficulty());

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
