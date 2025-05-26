using Models;

namespace AuctionServiceAPI.Services;

public interface IAuctionSyncPublisher
{
    Task PublishAuctionAsync(AuctionDTO auction);
}
