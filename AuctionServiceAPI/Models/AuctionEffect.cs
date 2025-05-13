using System;

namespace Auction
{
    public class AuctionEffect
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
    Upcoming,
    Active,
    Closed,
    Cancelled
}
