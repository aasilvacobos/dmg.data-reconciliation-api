namespace DMG.ProviderInvoicing.BL.Utility;

/// Utility for safely converting date/time values
public static class DateTimeUtility
{
	public static decimal ConvertHoursToMinutes(decimal hours)
	{
		try
		{
			// using BCL to avoid hard coding of conversion constants
			var hoursDouble = Convert.ToDouble(hours);
			var hoursTimeSpan = TimeSpan.FromHours(hoursDouble);
			var minutesDouble = hoursTimeSpan.TotalMinutes;
			return Convert.ToDecimal(minutesDouble);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static decimal ConvertSecondsToHours(long seconds)
	{
		try
		{
			return (decimal)TimeSpan.FromSeconds(seconds).TotalHours;
		}
		catch (Exception)
		{
			return 0;
		}
	}
}