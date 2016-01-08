namespace HitObjects
{
    //Represents a hitobject that can return a list of its hit locations and
    //hit times
    interface HitObjectWrapper
    {
        //Both of these methods must return a list of the same length

        //IMPORTANT: Only returns x-coordinates (as this is a ctb calculator)
        double[] GetHitLocations();
        double[] GetHitTimes();
    }
}
