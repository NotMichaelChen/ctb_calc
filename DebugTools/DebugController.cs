using System;
using System.Collections.Generic;
using System.IO;

using Structures;

namespace DebugTools
{
    public class DebugController
    {
        readonly string[] commandlist;
        
        public DebugController()
        {
            //Checks if the file exists, then loads it if it does
            if(File.Exists("debugcommands.txt"))
            {
                using (StreamReader sr = new StreamReader("debugcommands.txt"))
                {
                    string filecontents = sr.ReadToEnd();
                    commandlist = filecontents.Split('\n');
                    commandlist = Dewlib.TrimStringArray(commandlist);
                }
            }
            else
            {
                commandlist = new string[0];
            }
        }
        
        //Either load the custom-defined directory or file, overriding the custom arguments,
        //or load the default files
        //Note that if a directory is specified, only .osu files can be in it
        public string[] LoadCustom(string[] files)
        {
            foreach(string command in commandlist)
            {
                if(command.StartsWith("//", StringComparison.CurrentCulture))
                    continue;
                
                string[] pair = command.Split('=');
                if(pair.Length < 2)
                    continue;
                
                if(pair[0].ToLower() == "custompath")
                {
                    string path = pair[1];
                    //The only responsibility this method has is making sure that the files/directories exists
                    //Valid checking is up to the other objects using the loaded files
                    if(File.Exists(path))
                        return new string[] {path};
                    else if(Directory.Exists(path))
                    {
                        return Directory.GetFiles(path);
                    }
                    else
                        throw new DirectoryNotFoundException("Error: Custom file/directory not found\n" +
                                                             "path=" + path);
                }
            }
            
            return files;
        }
        
        public void WriteDebug(DiffCalc[] songs)
        {
            foreach(string command in commandlist)
            {
                if(command.StartsWith("//", StringComparison.CurrentCulture))
                    continue;
                
                string[] pair = command.Split('=');
                if(pair.Length < 2)
                    continue;
                
                if(pair[0].ToLower() == "printdebug" && pair[1].ToLower() == "true")
                {
                    Console.WriteLine("Writing Debug...");
                    
                    int donecount = 0;
                    Console.Write("0%\r");
                    Directory.CreateDirectory("debug");
                    
                    foreach(DiffCalc calc in songs)
                    {
                        HitPoint[] notes = calc.GetNoteDifficulty();
                        List<HitPoint> sortednotes = new List<HitPoint>(notes);
                        
                        bool sortbydifficulty = this.IsSortDifficulty();
                        if(sortbydifficulty)
                            sortednotes.Sort(HitPoint.CompareDifficulty);
                        else
                            sortednotes.Sort(HitPoint.CompareTime);
                        
                        string filepath = calc.GetBeatmapTitle() + ".txt";
                        
                        //Make sure files don't have invalid characters in the name
                        filepath = "debug//" + filepath.Replace("/", "").Replace("\"", "\'");
                        StreamWriter debugfile = new StreamWriter(filepath);
                        
                        debugfile.WriteLine("[Difficulty]".PadRight(21) + "[Time]");
                        foreach(HitPoint notepoint in sortednotes)
                        {
                            string leftvalue = notepoint.HitDifficulty.ToString();
                            debugfile.WriteLine(leftvalue.PadRight(21) + notepoint.HitTime);
                        }
                        
                        debugfile.Close();
                        donecount++;
                        Console.Write(Math.Round((double)donecount * 100 / songs.Length) + "%\r");
                    }
                    
                    break;
                }
            }
        }

        //Returns whether the WriteDebug Method should sort based on difficulty or time
        //Returns true if for difficulty, and false for time
        //If no "sort" tag is found, then default to true        
        private bool IsSortDifficulty()
        {
            foreach(string command in commandlist)
            {
                if(command.StartsWith("//", StringComparison.CurrentCulture))
                    continue;
                
                string[] pair = command.Split('=');
                if(pair.Length < 2)
                    continue;
                
                if(pair[0].ToLower() == "sort" && pair[1].ToLower() == "difficulty")
                {
                    return true;
                }
                else if(pair[0].ToLower() == "sort" && pair[1].ToLower() == "time")
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
