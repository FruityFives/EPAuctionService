using Models;

namespace AuctionServiceAPI.Services;

public interface IAuctionSyncPublisher
{
    Task PublishAuctionAsync(AuctionSyncDTO auction);
}
