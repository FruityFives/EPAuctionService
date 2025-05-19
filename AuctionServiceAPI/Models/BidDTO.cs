namespace Models;

public class BidDTO
{
    public Guid BidId { get; set; }
    
    public Guid UserId { get; set; }   // Bruger-id fra bruger-domænet
    public Guid AuctionId { get; set; }

    public double Amount { get; set; }
    public DateTime Timestamp { get; set; }
}
