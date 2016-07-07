using System;

using Structures;

namespace HitObjectInterpreter
{
    static class HitObjectParser
    {
        //Gets the property requested from the hitobject, specified by index
        //null is returned if the property is not found
        static public string GetProperty(string hitobject, string property)
        {
            property = property.ToUpper();

            //Divides the hitobject string into an array seperated by commas
            string[] hobject = hitobject.Split(',');

            int tag = -1;

            //TODO: Cleanup the property-selection code

            //Standard tags that all hitobjects have
            if(property == "X")
                tag = 0;
            else if(property == "Y")
                tag = 1;
            else if(property == "TIME")
                tag = 2;
            else if(property == "TYPE")
                tag = 3;
            else if(property == "HITSOUND")
                tag = 4;

            else
            {
                //If it's not one of the previous tags, then the location of the tag depends on the hitobject type
                //Be careful, GetHitObjectType calls this method, so make sure to avoid infinite recursion
                HitObjectType objecttype = GetHitObjectType(hitobject);

                if(objecttype == HitObjectType.Circle)
                {
                    if(property == "ADDITION")
                       tag = 5;
                }
                else if(objecttype == HitObjectType.Slider)
                {
                    //Special case: Slidertype contains info about the slidertype and its control points
                    //so I have separated it into two tags
                    //Ex. B|380:120|332:96|332:96|304:124

                    //The slidertype tag is the first char in the entire tag
                    if(property == "SLIDERTYPE")
                        return hobject[5][0].ToString();
                    //Custom tag: represents the control points within a slider
                    //is everything after the slidertype char
                    else if(property == "CONTROLPOINTS")
                        return hobject[5].Substring(2);

                    else if(property == "REPEAT")
                        tag = 6;
                    else if(property == "PIXELLENGTH")
                        tag = 7;
                    else if(property == "EDGEHITSOUND")
                        tag = 8;
                    else if(property == "EDGEADDITION")
                        tag = 9;
                    else if(property == "ADDITION")
                        tag = 10;
                }
                else if(objecttype == HitObjectType.Spinner)
                {
                    if(property == "ENDTIME")
                        tag = 5;
                    else if(property == "ADDITION")
                        tag = 6;
                }
            }

            //Protects against accessing a tag that's out of bounds
            //Not an exception since this method returns null if the tag wasn't found
            if(tag == -1 || tag >= hobject.Length)
                return null;

            return hobject[tag];
        }

        //Gets the type of hitobject specified at index
        static public HitObjectType GetHitObjectType(string hitobject)
        {
            //Get the hitobject type
            string type = GetProperty(hitobject, "type");

            BinaryString typeid = new BinaryString(Convert.ToInt32(type));

            //Binary 1
            if(typeid.GetBit(0) == 1)
            {
                return HitObjectType.Circle;
            }
            //Binary 2
            else if(typeid.GetBit(1) == 1)
            {
                return HitObjectType.Slider;
            }
            //Binary 8
            else if(typeid.GetBit(3) == 1)
            {
                return HitObjectType.Spinner;
            }
            else throw new ArgumentException("Hitobject type is not valid");
        }
    }
}
