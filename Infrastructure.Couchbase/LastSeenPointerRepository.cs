using Couchbase.Core.Exceptions.KeyValue;
using Infrastructure.Repository.Core;
using MelbergFramework.Infrastructure.Couchbase;

namespace Infrastructure.Couchbase;
public class LastSeenPointerRepository : BaseRepository, ILastSeenPointerRepository
{
    public LastSeenPointerRepository(IBucketFactory factory) : base("pointer", factory)
    {
    }

    public async Task<long> GetLastSeenTimeAsync(string hexValue)
    {
        try
        {
            var result = await Collection.GetAsync(hexValue);
            return result.ContentAs<long>();
        }
        catch (DocumentNotFoundException)
        {
            return 0;
        }

    }

    public Task SetLastSeenTimeAsync(string hexValue, long lastSeenMinute)
    {
        return Collection.UpsertAsync(hexValue,lastSeenMinute);
    }
}
