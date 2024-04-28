using Domain;
using Infrastructure.Repository.Core;
using MelbergFramework.Infrastructure.Couchbase;

namespace Infrastructure.Couchbase;
public class LastSeenPointerRepository : BaseRepository, ILastSeenPointerRepository
{
    public LastSeenPointerRepository(IBucketFactory factory) : base("pointer", factory)
    {
    }

    public async Task<PlaneMinimal> GetLastSeenRecordAsync(string hexValue)
    {
        try
        {
            var result = await Collection.GetAsync(hexValue);
            return result.ContentAs<PlaneMinimal>();
        }
        catch (Exception)
        {
            return new PlaneMinimal();
        }
    }

    public Task SetLastSeenRecordAsync(PlaneMinimal record)
    {
        return Collection.UpsertAsync(record.HexValue,record);
    }
}
