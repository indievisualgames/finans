using System.Linq;
using System;

// Create utility classes for common operations
public static class ValidationUtils
{
    public static bool ValidatePin(string pin)
    {
        if (string.IsNullOrEmpty(pin)) return false;
        if (pin.Length != Params.pinCharLimit) return false;
        return pin.All(char.IsDigit);
    }

    public static bool ValidateName(string name, int minLength = 4)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Trim().Length >= minLength;
    }
}

public static class DateUtils
{
    public static string FormatLevelNumber(int level)
    {
        return level < 10 ? $"0{level}" : level.ToString();
    }

    public static bool IsDateValid(DateTime gameDate, DateTime currentDate)
    {
        return DateTime.Compare(gameDate.Date, currentDate.Date) <= 0;
    }
}