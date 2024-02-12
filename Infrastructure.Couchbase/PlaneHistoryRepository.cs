using Couchbase.Core.Exceptions.KeyValue;
using Domain;
using Infrastructure.Repository.Core;
using MelbergFramework.Infrastructure.Couchbase;

namespace Infrastructure.Couchbase;

public class PlaneHistoryRepository : BaseRepository, IPlaneHistoryRepository
{
    public PlaneHistoryRepository(IBucketFactory factory) : base("long_term", factory)
    {
    }

    public Task StorePlaneHistory(string hexValue, long minuteInSeconds, PlaneDataRecordLink model) =>
        Collection.UpsertAsync(ToKey(hexValue,minuteInSeconds),model);
    
    public async Task<PlaneDataRecordLink> GetPlaneHistory(string hexValue, long minuteInSeconds)
    {
        try
        {
            var result = await Collection.GetAsync(ToKey(hexValue, minuteInSeconds)) ;
            return result.ContentAs<PlaneDataRecordLink>() ?? new PlaneDataRecordLink();
        }
        catch (DocumentNotFoundException)
        {
            return new PlaneDataRecordLink{};
        }
    }

    private static string ToKey(string hexValue, long minute) => $"{hexValue}_{minute}";
}