namespace Models;

public class AuctionEffectDTO
{
    public Guid EffectId { get; set; }
    public bool IsSold { get; set; }
    public Guid? WinnerUserId { get; set; }    
    public decimal? FinalPrice { get; set; }   
}
