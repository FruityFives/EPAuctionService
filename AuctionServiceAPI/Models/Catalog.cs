namespace AuctionServiceAPI.Models;

public class Catalog
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<Auction> Auctions { get; set; } = new();
}