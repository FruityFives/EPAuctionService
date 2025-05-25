namespace Models;

public class AuctionDTO
{
    public Guid EffectId { get; set; }
    public bool IsSold { get; set; }
    public Guid? WinnerUserId { get; set; }    
    public decimal? FinalPrice { get; set; }   
}
