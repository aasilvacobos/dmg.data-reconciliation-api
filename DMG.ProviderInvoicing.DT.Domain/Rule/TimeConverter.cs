using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class TimeConverter 
{
    public static AdjustmentMinutes ToAdjustmentMinutes(AdjustmentSeconds adjustmentSeconds) 
    {
        try 
        {
            // using BCL to avoid hard coding of conversion constants
            var secondsTimeSpan = TimeSpan.FromSeconds((float) adjustmentSeconds.Value);
            return new AdjustmentMinutes(Convert.ToInt32(secondsTimeSpan.TotalMinutes));
        } 
        catch (Exception) 
        {  
            return new AdjustmentMinutes(0);
        }
    }

    public static decimal ConvertMinutesToHours(int minutes) 
    {
        try 
        {
            // using BCL to avoid hard coding of conversion constants
            var minutesTimeSpan = TimeSpan.FromMinutes((float) minutes);
            var hoursDouble = minutesTimeSpan.TotalHours;
            return Convert.ToDecimal(hoursDouble);
        } 
        catch (Exception) 
        {
            return 0;
        }
    }
}