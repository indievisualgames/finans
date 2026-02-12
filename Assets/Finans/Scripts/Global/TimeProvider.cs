using System;

public class TimeProvider : IClock
{
	public DateTime UtcNow => DateTime.UtcNow;

	public DateTime NowInTimeZone(string timeZoneId)
	{
		try
		{
			if (string.IsNullOrEmpty(timeZoneId)) return DateTime.Now;
			TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			return TimeZoneInfo.ConvertTime(DateTime.UtcNow, tzi);
		}
		catch
		{
			return DateTime.Now;
		}
	}
}








