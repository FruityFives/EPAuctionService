namespace Models;

public class Catalog
{
    public Guid CatalogId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public CatalogStatus Status { get; set; } = CatalogStatus.Active;
    public List<Auction> Auctions { get; set; } = new(); //Måske fjerner vi denne
}

public enum CatalogStatus
{
    Active, // Kataloget er aktivt og kan indeholde auktioner
    Closed, // Kataloget er lukket og kan ikke indeholde auktioner
}