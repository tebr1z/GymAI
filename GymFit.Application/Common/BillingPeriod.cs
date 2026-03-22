namespace GymFit.Application.Common;

public static class BillingPeriod
{
    public static string CurrentUtcMonthKey(DateTime? utcNow = null) =>
        (utcNow ?? DateTime.UtcNow).ToString("yyyy-MM");
}
