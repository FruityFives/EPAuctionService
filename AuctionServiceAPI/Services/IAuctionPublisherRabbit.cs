using Models;

namespace AuctionServiceAPI.Services
{
    public interface IAuctionPublisherRabbit
    {
        Task PublishAuctionAsync(AuctionDTO auction);
    }
}
