using System;
using System.Text;

namespace Structures
{
    /// <summary>
    /// Represents a number as a binary number as a string
    /// Allows individual bit extraction
    /// </summary>
    public class BinaryString
    {
        string binary;

        /// <summary>
        /// Converts an int into a binary number represented by a string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public BinaryString(int num)
        {
            if(num == 0)
            {
                binary = "0";
                return;
            }

            int tempnum = num;
            StringBuilder tempstring = new StringBuilder();

            //Get the largest power that's still under num
            int power = 0;
            while(Math.Pow(2, power) <= num)
                power++;
            //Stay less than num, but make sure power isn't negative
            if(power > 0)
                power--;

            for(int i = power; i >= 0; i--)
            {
                if(Math.Pow(2, i) <= tempnum)
                {
                    tempnum -= Convert.ToInt32(Math.Pow(2, i));
                    tempstring.Append('1');
                }
                else
                {
                    tempstring.Append('0');
                }
            }

            binary = tempstring.ToString();
        }

        //Get the entire binary number as a string
        public string GetBinary()
        {
            return binary;
        }

        /// <summary>
        /// Get the bit specified
        /// Counts from the least-significant-bit. So if our number is 11001, GetBit(0) returns 1, GetBit(1), returns 0...
        /// </summary>
        /// <param name="index">Which bit to get</param>
        /// <returns>The bit requested. -1 is returned if the index doesn't exist</returns>
        public int GetBit(int index)
        {
            if(index >= binary.Length || index < 0)
                return -1;

            //Start from the least-significant-bit, and count up from there
            return binary[binary.Length - 1 - index] - 48;
        }
    }
}
