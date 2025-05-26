using Models;
namespace AuctionServiceAPI.Services;
public interface IStoragePublisherRabbit
{
    Task PublishAuctionAsync(AuctionEffectDTO auction);
}