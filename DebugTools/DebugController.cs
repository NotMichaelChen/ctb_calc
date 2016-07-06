using System;
using System.IO;

namespace DebugTools
{
    public class DebugController
    {
        string[] commands;
        
        public DebugController()
        {
            //Checks if the file exists, then loads it if it does
            if(File.Exists("debugcommands.txt"))
            {
                using (StreamReader sr = new StreamReader("debugcommands.txt"))
                {
                    string filecontents = sr.ReadToEnd();
                    commands = filecontents.Split(new char[] {'\n'});
                    commands = Dewlib.TrimStringArray(commands);
                }
            }
        }
    }
}
