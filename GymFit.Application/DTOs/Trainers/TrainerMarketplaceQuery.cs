namespace GymFit.Application.DTOs.Trainers;

public sealed class TrainerMarketplaceQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public decimal? MinRating { get; set; }
    public decimal? MaxPricePerMonth { get; set; }
}
