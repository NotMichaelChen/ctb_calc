using System;

namespace Structures
{
    //Stores information about a single hit location, or a single catchable fruit
    //A hitcircle has one hitpoint, a slider has many hitpoints (slider head, ticks, and slider tail)
    //A spinner has no hit point but a hit time (though this struct is unable to handle spinners)
    public struct HitPoint : IComparable
    {
        public int HitLocation;
        public int HitTime;
        public double HitDifficulty;
        
        public HitPoint(int location, int time, double difficulty)
        {
            HitLocation = location;
            HitTime = time;
            HitDifficulty = difficulty;
        }
        
        public int CompareTo(object obj)
        {
            if(obj is HitPoint)
            {
                HitPoint otherpoint = (HitPoint)obj;
                if(this.HitDifficulty == otherpoint.HitDifficulty)
                    return 0;
                else if(this.HitDifficulty < otherpoint.HitDifficulty)
                    return -1;
                else
                    return 1;
            }
            else throw new Exception("Error: object is not hitpoint");
        }
    }
}
