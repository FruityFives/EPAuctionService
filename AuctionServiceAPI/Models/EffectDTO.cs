using System;

namespace Auction
{
    public class EffectDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal AssessmentPrice { get; set; }
        public string ConditionReport { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public AuctionStatus Status { get; set; }
    }
}
public enum AuctionStatus
{
    Upcoming, // Vi skal bruge denne når vi opretter en auktion
    Active, // Auktionen er aktiv og kan bydes på
    Closed, // Auktionen er lukket og kan ikke bydes på
    Cancelled // Auktionen er annulleret fordi vi ikke æslkger produktet
}
