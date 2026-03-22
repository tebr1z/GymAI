namespace GymFit.Application.DTOs.Trainers;

/// <summary>
/// Booking request. <see cref="ExpectedPrice"/> should match the trainer's current monthly price
/// from the last marketplace response to detect price changes before payment.
/// </summary>
public sealed class BookTrainerDto
{
    public Guid TrainerProfileId { get; set; }

    /// <summary>Optional. When set, must equal the trainer's <c>PricePerMonth</c> or booking fails.</summary>
    public decimal? ExpectedPrice { get; set; }
}
