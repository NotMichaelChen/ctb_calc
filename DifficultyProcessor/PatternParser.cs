using System;
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
            double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
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
                    isprevhyper = catcher.PercentHyper(Math.Abs(hitpositions[i-1] - hitpositions[i-2]) / (hittimes[i-1] - hittimes[i-2])) > 1;
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
            double circlesize = Convert.ToDouble(map.GetTag("Difficulty", "CircleSize"));
            CatcherInfo catcher = new CatcherInfo(circlesize);
            
            List<int> SGtimes = new List<int>();
            
            for(int i = 1; i < hitpositions.Length; i++)
            {
                if(hittimes[i] == hittimes[i-1] || Math.Abs(hitpositions[i] - hitpositions[i-1]) < 110)
                    continue;
                
                double velocity = Math.Abs(hitpositions[i] - hitpositions[i-1]) / (double)(hittimes[i] - hittimes[i-1]);
                velocity = catcher.PercentHyper(velocity);
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
                
                while(lastnonmoveindex > 0)
                {
                    //Update the "bounds" of the pattern
                    if(hitpositions[lastnonmoveindex-1] < leftmost)
                        leftmost = hitpositions[lastnonmoveindex-1];
                    else if(hitpositions[lastnonmoveindex-1] > rightmost)
                        rightmost = hitpositions[lastnonmoveindex-1];
                    
                    //Actual "check" of the loop
                    if(rightmost - leftmost > (catcher.CatcherSize * 0.25))
                       break;
                    
                    lastnonmoveindex--;
                }
                
                int nonmovetime = hittimes[i] - hittimes[lastnonmoveindex];
                
                if(nonmovetime > (catcher.CatcherSize / 2))
                    SGtimes.Add(hittimes[i]);
            }
            
            return SGtimes.ToArray();
        }
    }
}
