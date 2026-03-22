namespace GymFit.Domain.Entities;

/// <summary>Monthly AI request counter per user (UTC period key).</summary>
public class AiUsageLedger : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>UTC month bucket, format yyyy-MM.</summary>
    public string PeriodKey { get; set; } = string.Empty;

    public int RequestCount { get; set; }
}
