namespace AuctionServiceAPI.Models;

public class BidDTO
{
    public Guid Id { get; set; }

    // Bruger-id fra bruger-domænet
    public Guid UserId { get; set; }

    public double Price { get; set; }
    public DateTime Timestamp { get; set; }
}
