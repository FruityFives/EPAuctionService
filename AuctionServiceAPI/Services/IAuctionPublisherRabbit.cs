using Models;

namespace AuctionServiceAPI.Services
{
    public interface IAuctionPublisherRabbit
    {
        Task PublishAuctionAsync(AuctionSyncDTO auction);
    }
}
