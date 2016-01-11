using System;

namespace HitObjects
{
    //Dummy class, since spinners don't contribute a meaningful hit location
    //Makes things more organized when parsing hitobjects though, so it gets implemented
    public class Spinner : HitObjectWrapper
    {
        public double[] GetHitLocations()
        {
            return new double[0];
        }

        public double[] GetHitTimes()
        {
            return new double[0];
        }
    }
}
