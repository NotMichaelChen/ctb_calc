using System;

//Holds relevant information about a catcher
public class CatcherInfo
{
    private double CS;
    
    public CatcherInfo(double aCS)
    {
        CS = aCS;
    }
    
    public int GetCatcherSize()
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
        
        return (int)size;
    }
}
