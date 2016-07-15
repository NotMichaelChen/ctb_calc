using System;
using System.Collections.Generic;

//Holds relevant information about a catcher
public class CatcherInfo
{
    private double CS;
    //This is the constant number that distance-time must be to create a hyper
    private int hyperconstant;
    private int catcherwidth;
    
    public Direction CurDirection;
    
    public enum Direction
    {
        Left,
        Right,
        Stop
    }
    
    public CatcherInfo(double aCS)
    {
        CS = aCS;
        CurDirection = Direction.Stop;
        
        CalculateCatcherSize();
        GenerateHyperConstant();
    }
    
    public int CatcherSize
    {
        get
        {
            return catcherwidth;
        }
    }
    
    //Returns what percent the given velocity is to a pixel-jump
    /*public double PercentHyper(double velocity)
    {
        //Obtained from testing, may not be 100% exact
        return velocity / (-0.07 * CS + 1.69);
    }*/
    
    public double PercentHyper(double distance, double time)
    {
        return distance / (time + hyperconstant);
    }
    
    private void CalculateCatcherSize()
    {
        CS = Dewlib.RestrictRange(CS, 0, 10);

        //Calculate the integer part of CS first, then modify later based on the decimal
        double size = 144 - 12 * Math.Floor(CS);

        //If CS has a decimal place
        if(CS % 1 != 0)
        {
            //Avoid precision bugs - round to one decimal place
            double CSdecimal = Math.Round(CS - Math.Floor(CS), 1);
            
            if(CSdecimal == 0.2)
                size -= 2;
            else if(0.3 <= CSdecimal && CSdecimal <= 0.4)
                size -= 4;
            else if(0.5 <= CSdecimal && CSdecimal <= 0.6)
                size -= 6;
            else if(CSdecimal == 0.7)
                size -= 8;
            else if(0.8 <= CSdecimal && CSdecimal <= 0.9)
                size -= 10;
        }
        
        catcherwidth = (int)size;
    }
    
    private void GenerateHyperConstant()
    {
        int distancetimedifference = 86;
        //Easier to increment by 1 then by 0.1
        for(double i = 0; i <= 100; i++)
        {
            if(Math.Round(i / 10, 1) == Math.Round(CS, 1))
            {
                hyperconstant = distancetimedifference;
                return;
            }
            
            //Prepare for the next incremented CS
            
            //If the numeric portion of the CS is even, that means it takes 7 steps
            //to reach the odd CS
            if((int)(i/10) % 2 == 0)
            {
                int CSdecimal = (int)i % 10;
                if(CSdecimal != 1 && CSdecimal != 5 && CSdecimal != 9)
                    distancetimedifference--;
            }
            //Otherwise if the numeric portion of the CS is odd, that means it takes 8
            //steps to reach the even CS
            else
            {
                int CSdecimal = (int)i % 10;
                if(CSdecimal != 3 && CSdecimal != 7)
                    distancetimedifference--;
            }
        }
        
        throw new ArgumentException("Error, CS is not valid\n" +
                                    "CS=" + CS);
    }
}
