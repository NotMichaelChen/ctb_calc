using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using BeatmapInfo;
using CustomExceptions;
using DebugTools;

//TODO: Overhaul exception system

public class Program
{
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        
        DebugController debugger = new DebugController();
        
        //Load in custom beatmaps if specified
        args = debugger.LoadCustom(args);
        
        //Display a message if no files are specified
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
            List<DiffCalc> calculators = new List<DiffCalc>();
            Stopwatch timer = new Stopwatch();
            try
            {
                int count = 0;
                foreach(string name in args)
                {
                    timer.Start();

                    Beatmap map = new Beatmap(name);
                    DiffCalc calc;
                    try
                    {
                        calc = new DiffCalc(map);
                    }
                    catch(InvalidBeatmapException)
                    {
                        //Skip this beatmap if it's not a standard or ctb map, but only if
                        //it was loaded through debug
                        if(debugger.IsLoadCustom())
                            continue;
                        else
                            throw;
                    }
                    
                    string title = calc.GetBeatmapTitle() + ": \t";
                    double difficulty = calc.GetDifficulty();

                    timer.Stop();
                    
                    title += timer.ElapsedMilliseconds;
                    beatmaps[difficulty] = title;
                    calculators.Add(calc);
                    
                    timer.Reset();
                    count++;
                    Console.Write(Math.Round((double)count * 100 / args.Length) + "%\r");
                }
                
                Console.WriteLine("\n");
                for(int i = beatmaps.Count - 1; i >= 0; i--)
                {
                    string[] titleandtime = beatmaps.Values[i].Split('\t');
                    Console.WriteLine(titleandtime[0] + beatmaps.Keys[i]);
                    Console.WriteLine("Calculation Time (ms): " + titleandtime[1] + "\n");
                }
                
                debugger.WriteDebug(calculators.ToArray());

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
