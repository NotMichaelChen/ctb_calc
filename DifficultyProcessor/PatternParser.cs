using System;
using System.Globalization;
using System.Collections.Generic;

using BeatmapInfo;

using Direction = CatcherInfo.Direction;

namespace DifficultyProcessor
{
    public class PatternParser
    {
        private readonly Beatmap map;
        private readonly int[] hitpositions;
        private readonly int[] hittimes;
        
        public PatternParser(Beatmap givenmap, int[] positions, int[] times)
        {
            map = givenmap;
            hitpositions = positions;
            hittimes = times;
        }
        
        //Each hitobject marked as a "DC" means that it requires a directional change to catch
        public int[] GetDirectionalChangeTimes()
        {
            double circlesize = Double.Parse(map.GetTag("Difficulty", "CircleSize"), CultureInfo.InvariantCulture);
            CatcherInfo catcher = new CatcherInfo(circlesize);
            
            List<int> DCtimes = new List<int>();
            
            Direction prevnotedir = catcher.CurDirection;
            for(int i = 1; i < hitpositions.Length; i++)
            {
                if(hitpositions[i] == hitpositions[i-1])
                    continue;
                
                Direction notedirection;
                
                if(hitpositions[i] - hitpositions[i-1] > 0)
                    notedirection = Direction.Right;
                else
                    notedirection = Direction.Left;
                
                bool isprevhyper;
                if(i > 1 && hittimes[i-1] != hittimes[i-2])
                    isprevhyper = catcher.PercentHyper(Math.Abs(hitpositions[i-1] - hitpositions[i-2]), hittimes[i-1] - hittimes[i-2]) > 1;
                else
                    isprevhyper = false;
                
                int distance = Math.Abs(hitpositions[i]-hitpositions[i-1]);
                double checkedsize = catcher.CatcherSize;
                if(isprevhyper)
                    checkedsize /= 2;
                if(notedirection != catcher.CurDirection && (distance > checkedsize || notedirection == prevnotedir))
                {
                    catcher.CurDirection = notedirection;
                    DCtimes.Add(hittimes[i]);
                }
                
                prevnotedir = catcher.CurDirection;
            }
            
            return DCtimes.ToArray();
        }
        
        public int[] GetStopGoTimes()
        {
            double circlesize = Double.Parse(map.GetTag("Difficulty", "CircleSize"), CultureInfo.InvariantCulture);
            CatcherInfo catcher = new CatcherInfo(circlesize);
            
            List<int> SGtimes = new List<int>();
            
            for(int i = 1; i < hitpositions.Length; i++)
            {
                if(hittimes[i] == hittimes[i-1] || Math.Abs(hitpositions[i] - hitpositions[i-1]) < 110)
                    continue;
                
                double velocity = catcher.PercentHyper(Math.Abs(hitpositions[i] - hitpositions[i-1]), hittimes[i] - hittimes[i-1]);
                //Skip if not a hyper
                if(velocity <= 1)
                    continue;
                
                //Skip if last two notes can't be caught together
                if(i < 2 || hitpositions[i-1] - hitpositions[i-2] > catcher.CatcherSize)
                    continue;
                
                //Represents the last note that required no movement before the hyper jump
                int lastnonmoveindex = i-2;
                
                int leftmost = hitpositions[i-1] > hitpositions[i-2] ? hitpositions[i-2] : hitpositions[i-1];
                int rightmost = hitpositions[i-1] > hitpositions[i-2] ? hitpositions[i-1] : hitpositions[i-2];
                
                int templeft = leftmost;
                int tempright = rightmost;
                while(lastnonmoveindex > 0)
                {    
                    //Update the "bounds" of the pattern
                    if(hitpositions[lastnonmoveindex-1] < leftmost)
                        templeft = hitpositions[lastnonmoveindex-1];
                    else if(hitpositions[lastnonmoveindex-1] > rightmost)
                        tempright = hitpositions[lastnonmoveindex-1];
                    
                    //Actual "check" of the loop
                    if(tempright - templeft > (catcher.CatcherSize))
                       break;
                    
                    leftmost = templeft;
                    rightmost = tempright;
                    lastnonmoveindex--;
                }
                
                int nonmovetime = hittimes[i] - hittimes[lastnonmoveindex];
                
                if((rightmost - leftmost) / (double)nonmovetime < 0.5)
                    SGtimes.Add(hittimes[i]);
            }
            
            return SGtimes.ToArray();
        }
    }
}
