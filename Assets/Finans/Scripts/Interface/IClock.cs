using System;

public interface IClock
{
	DateTime UtcNow { get; }
	DateTime NowInTimeZone(string timeZoneId);
}








