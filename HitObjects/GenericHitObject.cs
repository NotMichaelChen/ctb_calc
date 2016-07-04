using System;

using BeatmapInfo;
using HitObjectInterpreter;

namespace HitObjects
{
    //Represents a generic hitobject that can be used to get hit locations and
    //hit times regardless of the type of the hit object
    //Implementing HitObjectWrapper is not necessarily needed, but it makes sure
    //that GenericHitObject has the same methods as the other hitobjects
    public class GenericHitObject : HitObjectWrapper
    {
        readonly HitObjectWrapper hitobject;
        
        //Get the correct type of hitobject to put into the hitobject variable
        public GenericHitObject(string id, Beatmap map)
        {
            HitObjectType objecttype = HitObjectParser.GetHitObjectType(id);
            
            if(objecttype == HitObjectType.Circle)
                hitobject = new Circle(id);
            else if(objecttype == HitObjectType.Slider)
            {
                string slidertype = HitObjectParser.GetProperty(id, "slidertype");
                if(slidertype == "L")
                    hitobject = new LinearSlider(id, map);
                //Special behavior is needed for passthrough sliders
                else if(slidertype == "P")
                {
                    //Treat the slider differently depending on the number of control points
                    string[] sliderpoints = HitObjectParser.GetProperty(id, "controlpoints").Split(new char[] {'|'});
                    if(sliderpoints.Length == 1)
                        hitobject = new LinearSlider(id, map);
                    else if(sliderpoints.Length == 2)
                        hitobject = new PassthroughSlider(id, map);
                    else
                        hitobject = new BezierSlider(id, map);
                }
                else if(slidertype == "B")
                    hitobject = new BezierSlider(id, map);
                else if(slidertype == "C")
                    hitobject = new CatmullSlider(id, map);
            }
            else if(objecttype == HitObjectType.Spinner)
                hitobject = new Spinner();
            else
                throw new ArgumentException("Error: id is invalid");
        }
        
        public int[] GetHitLocations()
        {
            return hitobject.GetHitLocations();
        }
        
        public int[] GetHitTimes()
        {
            return hitobject.GetHitTimes();
        }
    }
}
