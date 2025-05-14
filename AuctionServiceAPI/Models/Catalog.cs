namespace Models;

public class Catalog
{
    public Guid CatalogId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Auction> Auctions { get; set; } = new(); //Måske fjerner vi denne
}