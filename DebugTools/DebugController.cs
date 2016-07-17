using System;
using System.Collections.Generic;
using System.IO;

using Structures;
using DifficultyProcessor;

namespace DebugTools
{
    public class DebugController
    {
        string custompath;
        bool printdebug = false;
        bool issortdifficulty = true;
        
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
                this.FindCommands();
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
            //custom path is not empty
            if(custompath != null)
            {
                //The only responsibility this method has is making sure that the files/directories exists
                //Valid checking is up to the other objects using the loaded files
                if(File.Exists(custompath))
                    return new string[] {custompath};
                else if(Directory.Exists(custompath))
                {
                    return Directory.GetFiles(custompath, "*.osu");
                }
                else
                    throw new DirectoryNotFoundException("Error: Custom file/directory not found\n" +
                                                         "path=" + custompath);
            }
            return files;
        }
        
        public void WriteDebug(DiffCalc[] songs)
        {
            if(printdebug)
            {
                Console.WriteLine("Writing Debug...");
                
                int donecount = 0;
                Console.Write("0%\r");
                Directory.CreateDirectory("debug");
                
                foreach(DiffCalc calc in songs)
                {
                    HitPoint[] notes = calc.GetNoteDifficulty();
                    List<HitPoint> sortednotes = new List<HitPoint>(notes);
                    
                    if(issortdifficulty)
                        sortednotes.Sort(HitPoint.CompareDifficulty);
                    else
                        sortednotes.Sort(HitPoint.CompareTime);
                    
                    //Make sure files don't have invalid characters in the name
                    string filepath = Dewlib.MakeValidFilename(calc.GetBeatmapTitle() + ".txt");
                    
                    filepath = "debug//" + filepath;
                    StreamWriter debugfile = new StreamWriter(filepath);

                    //Write the header
                    debugfile.WriteLine("[Difficulty]".PadRight(21) + "[Time]".PadRight(9) + "[Index]");
                    foreach(HitPoint notepoint in sortednotes)
                    {
                        string difficulty = notepoint.HitDifficulty.ToString();
                        string time = notepoint.HitTime.ToString();
                        debugfile.WriteLine(difficulty.PadRight(21) + time.PadRight(9) + notepoint.HitID);
                    }
                    
                    debugfile.Close();
                    donecount++;
                    Console.Write(Math.Round((double)donecount * 100 / songs.Length) + "%\r");
                }
            }
        }
        
        public bool IsLoadCustom()
        {
            return custompath != null;
        }
        
        private void FindCommands()
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
                    issortdifficulty = true;
                }
                else if(pair[0].ToLower() == "sort" && pair[1].ToLower() == "time")
                {
                    issortdifficulty = false;
                }
                
                else if(pair[0].ToLower() == "custompath")
                {
                    custompath = pair[1];
                }
                
                else if(pair[0].ToLower() == "printdebug" && pair[1].ToLower() == "true")
                {
                    printdebug = true;
                }
                else if(pair[0].ToLower() == "printdebug" && pair[1].ToLower() == "false")
                {
                    printdebug = false;
                }
            }
        }
    }
}
