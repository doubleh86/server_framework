using System.Text;
using Force.Crc32;

namespace ServerFramework.CommonUtils.Helper;

public static class CommonHelper
{
    public static int GetDbId(string accountId, int dbCount)
    {
        if(dbCount <= 0)
            throw new ArgumentException("dbCount must be greater than 0");
        
        var crcCode = Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(accountId));
            
        var index = (int)(crcCode % dbCount);
        return index;
    }
    
    public static List<T> Shuffle<T>(List<T> listData)
    {
        var list = new List<T>(listData);
        for (var i = list.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Shared.Next(i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }

        return list;
    }
    
    
    public static (int, int, int) ParseStringTimeToInt(string timeString)
    {
        if (!int.TryParse(timeString, out var timeInt))
            throw new FormatException("Invalid time format.");
        
        if(timeInt <= 2359)
            timeInt *= 100;
        
        var hours = timeInt / 10000;
        var minutes = (timeInt / 100) % 100;
        var seconds = timeInt % 100;
        
        if (hours is < 0 or > 23 || minutes is < 0 or > 59 || seconds is < 0 or > 59)
            throw new ArgumentOutOfRangeException(nameof(timeString), "Time value is out of range.");
        
        return (hours, minutes, seconds);
    }

    public static bool CheckOverlappedDate(DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
    {
        return startDate1 <= endDate2 && endDate1 >= startDate2;
    }
}