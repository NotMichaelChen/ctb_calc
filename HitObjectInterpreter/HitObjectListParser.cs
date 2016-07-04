using System;

using BeatmapInfo;

namespace HitObjectInterpreter
{
    //Acts as a wrapper for the list of hitobjects in a beatmap
    //Uses the HitObjectParser class to perform most of the heavy-lifting
    class HitObjectListParser
    {
        //List of hitobjects that's been wrapped
        private readonly string[] hitobjects;

        // Gets the list of hitobjects from a beatmap object and stores it for later use
        public HitObjectListParser(Beatmap map)
        {
            this.hitobjects = map.GetSection("hitobjects");
        }

        //Gets the size of the hitobjects array
        public int GetSize()
        {
            return hitobjects.Length;
        }

        //Gets the hitobject id at the specified index
        public string GetHitObjectID(int index)
        {
            return hitobjects[index];
        }

        //The two methods below use HitObjectParser to perform the operation

        //Gets the property requested from the hitobject at the index
        public string GetProperty(int index, string property)
        {
            return HitObjectParser.GetProperty(hitobjects[index], property);
        }

        //Gets the type of the hitobject at the index
        public HitObjectType GetHitObjectType(int index)
        {
            return HitObjectParser.GetHitObjectType(hitobjects[index]);
        }
    }
}
