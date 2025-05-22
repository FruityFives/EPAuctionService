namespace Models;

public class AuctionDTO
{

    public Guid WinnerId { get; set; }

    public Guid EffectId { get; set; }

    public double FinalAmount { get; set; }
    
    public bool IsSold { get; set; }
    
}