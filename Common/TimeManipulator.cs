namespace Common;

public static class TimeManipulator
{
    public static long ToLastMinInSec(this long time)
    {
        var dateTime = DateTime.UnixEpoch.AddSeconds(time);        
        dateTime = dateTime.AddSeconds(0-dateTime.Second);
        var result = (long)(dateTime - DateTime.UnixEpoch).TotalSeconds;
        return result;
    } 
}