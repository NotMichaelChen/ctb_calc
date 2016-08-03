using System;
using System.Collections.Generic;

//Holds relevant information about a catcher
public class CatcherInfo
{
    private double CS;
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
    }

    public double CircleSize
    {
        get
        {
            return CS;
        }
    }
    
    public int CatcherSize
    {
        get
        {
            return catcherwidth;
        }
    }
    
    //Returns what percent the given velocity is to a pixel-jump
    public double PercentHyper(double distance, double time)
    {
        //This is the constant number that distance-time must be to create a hyper
        //Formula credit goes to CelegaS
        int hyperconstant = (int)Math.Round(86 - 7.5 * CS);
        return distance / (time + hyperconstant);
    }

    //Scales the CS exponentially when used in difficulty calculations
    public double GetCSMultiplier()
    {
        return Math.Pow(CS, 1.57) / (Math.Pow(10, 1.57) / 10);
    }

    private void CalculateCatcherSize()
    {
        CS = Dewlib.Clamp(CS, 0, 10);

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
}
