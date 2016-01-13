namespace HitObjects
{
    //Dummy class, since spinners don't contribute a meaningful hit location
    //Makes things more organized when parsing hitobjects though, so it gets implemented
    public class Spinner : HitObjectWrapper
    {
        public int[] GetHitLocations()
        {
            return new int[0];
        }

        public int[] GetHitTimes()
        {
            return new int[0];
        }
    }
}
