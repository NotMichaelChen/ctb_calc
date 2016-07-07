using System;
using System.IO;
using System.Collections.Generic;

namespace BeatmapInfo
{
    //Contains all information about an osu beatmap file and allows access to certain tags and sections
    public class Beatmap
    {
        //Array of strings that hold the entire file in separate lines
        private string[] filelines;
        //Holds the location of each section
        private BeatmapSections section;

        /****************
         *
         * Public Methods
         *
         ***************/

        /// Attempt to load the give filename and format it
        public Beatmap(string filename)
        {
            //Load the file into one string
            string file = LoadFile(filename);

            //Format this string into the filelines variable
            FormatFileString(file);

            //Load locations of each section into the BeatmapSections struct
            LoadSectionMarkers();
        }

        // Searches for a tag in a given section.
        // This method does not search for info in events, timingpoints, colours, or hitobjects.
        // If the tag is not found, the method returns null.
        public string GetTag(string sectionname, string tag)
        {
            sectionname = sectionname.ToUpper();

            //Get the correct line number for the given section
            int sectionline = section.GetTaggableSectionLine(sectionname);

            //If the section doesn't exist, or is not a taggable section, return null
            if(sectionline == -1)
                return null;

            //Searches through each line for the requested tag
            for(int i = sectionline+1; i < filelines.Length; i++)
            {
                //Section ends on empty line, so stop searching once you get to one
                if(filelines[i].Length == 0)
                    break;

                //Get a pair of strings, one side is the tag, other side is the value of the tag
                string[] pair = Dewlib.SplitFirst(filelines[i], ':');

                //Skip if the pair is invalid
                if(pair.Length != 2)
                {
                    continue;
                }

                //Trim the pair, since the tag value will have a space (e.g. Mode: 0)
                pair = Dewlib.TrimStringArray(pair);

                //Essentially if the tag is a match
                if(pair[0].ToUpper() == tag.ToUpper())
                {
                    //Return its value
                    return pair[1];
                }
            }
            //If nothing was found, return null
            return null;
        }

        //Get a string array from an entire section
        public string[] GetSection(string sectionname)
        {
            //Make a list of tags that will be returned
            List<string> tags = new List<string>();

            //Get where the section begins
            int sectionline = this.section.GetSectionLine(sectionname);

            //Loop through each line of the section
            //Start at sectionline + 1 to skip the section header itself
            for(int i = sectionline + 1; i < filelines.Length; i++)
            {
                //Exit the loop if we hit another section
                if(section.IsSection(filelines[i]))
                    break;

                //Otherwise add the line to the list
                tags.Add(filelines[i]);
            }

            string[] finaltags = tags.ToArray();
            finaltags = Dewlib.RemoveEmptyEntries(finaltags);

            //Return the list
            return finaltags;
        }

        /*************************
         *
         * Initialization Methods
         *
         ************************/

        /// <summary>
        /// Attempts to load a file, and if successful returns the contents in a string
        /// </summary>
        /// <param name="filename">The file to attempt to load</param>
        /// <returns>The contents of the file</returns>
        private string LoadFile(string filename)
        {
            //Checks if the file exists, then loads it if it does
            if(File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Format the file's contents into an array (each entry is one line), loads it into this.filelines, and trims each entry of whitespace.
        /// Empty lines are preserved (to determine when a section ends)
        /// </summary>
        /// <param name="filecontents">The contents of the file in a string</param>
        private void FormatFileString(string filecontents)
        {
            filelines = filecontents.Split('\n');
            filelines = Dewlib.TrimStringArray(filelines);
        }

        /// <summary>
        /// Locates .osu sections and their locations, then loads that info into a BeatmapSections struct.
        /// Nonpresent sections are given a location of -1 (see BeatmapSections)
        /// </summary>
        private void LoadSectionMarkers()
        {
            //Initialize the section member
            this.section = new BeatmapSections();

            //Search through each line, and if the line is a tag then set it
            for(int i = 0; i < this.filelines.Length; i++)
            {
                if(filelines[i] == "[General]")
                    section.General = i;
                else if(filelines[i] =="[Editor]")
                    section.Editor = i;
                else if(filelines[i] == "[Metadata]")
                    section.Metadata = i;
                else if(filelines[i] == "[Difficulty]")
                    section.Difficulty = i;
                else if(filelines[i] == "[Events]")
                    section.Events = i;
                else if(filelines[i] == "[TimingPoints]")
                    section.TimingPoints = i;
                else if(filelines[i] == "[Colours]")
                    section.Colours = i;
                else if(filelines[i] == "[HitObjects]")
                    section.HitObjects = i;
            }
        }
    }
}
