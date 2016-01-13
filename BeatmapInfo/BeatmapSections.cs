using System;

namespace BeatmapInfo
{
    public class BeatmapSections
    {
        int general, editor, metadata, difficulty, events, timingpoints, colours, hitobjects;

        /// Initializes the section variables so they can be written to later
        public BeatmapSections()
        {
            general = editor = metadata = difficulty = events = timingpoints = colours = hitobjects = -1;
        }

        //Gets the line of a section from a string
        //Return -1 if the section is invalid
        public int GetSectionLine(string section)
        {
            section = section.ToUpper();

            //See if it's a taggable section
            int taggedsection = GetTaggableSectionLine(section);
            if(taggedsection != -1)
                return taggedsection;

            //Otherwise search through the non-taggable sections
            else if(section == "EVENTS")
                return events;
            else if(section == "TIMINGPOINTS")
                return timingpoints;
            else if(section == "COLOURS")
                return colours;
            else if(section == "HITOBJECTS")
                return hitobjects;
            else return -1;
        }

        //Gets the line of a section, but only returns a value if it is a section with tags
        //Returns -1 if the section is invalid
        public int GetTaggableSectionLine(string section)
        {
            section = section.ToUpper();

            if(section == "GENERAL")
                return general;
            else if(section == "EDITOR")
                return editor;
            else if(section == "METADATA")
                return metadata;
            else if(section == "DIFFICULTY")
                return difficulty;
            else return -1;
        }

        //Returns whether the given string is actually a section
        public bool IsSection(string section)
        {
            //Cannot be a section if it's not long enough to contain [ and ]
            if(section.Length < 2)
                return false;

            //Get rid of the [ and ] and check whether the resulting string is a section
            return GetSectionLine(section.Substring(1, section.Length - 2)) != -1;
        }

        public int General
        {
            get { return general; }
            set
            {
                if(general == -1)
                    general = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int Editor
        {
            get { return editor; }
            set
            {
                if(editor == -1)
                    editor = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int Metadata
        {
            get { return metadata; }
            set
            {
                if(metadata == -1)
                    metadata = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int Difficulty
        {
            get { return difficulty; }
            set
            {
                if(difficulty == -1)
                    difficulty = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int Events
        {
            get { return events; }
            set
            {
                if(events == -1)
                    events = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int TimingPoints
        {
            get { return timingpoints; }
            set
            {
                if(timingpoints == -1)
                    timingpoints = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int Colours
        {
            get { return colours; }
            set
            {
                if(colours == -1)
                    colours = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }

        public int HitObjects
        {
            get { return hitobjects; }
            set
            {
                if(hitobjects == -1)
                    hitobjects = value;
                else
                    throw new InvalidOperationException("Trying to write to BeatmapSection");
            }
        }
    }
}
