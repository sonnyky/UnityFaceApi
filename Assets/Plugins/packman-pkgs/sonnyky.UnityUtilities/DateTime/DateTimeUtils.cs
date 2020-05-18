
using System;

public static class DateTimeUtils 
{
    public static bool DateTimeFormatCheck(string inputString)
    {
        DateTime dDate;

        if (DateTime.TryParse(inputString, out dDate))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
