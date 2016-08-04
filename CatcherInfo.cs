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
        //Formula credit goes to CelegaS
        catcherwidth = (int)(144 - Math.Round(6 * CS - 0.2) * 2);
    }
}
