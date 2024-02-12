using Domain;
using Infrastructure.Repository.Core;

namespace DomainService;

public interface IAccessDomainService
{
    Task<PlaneDataRecordLink> RetrievePlaneHistory(string hexValue, long time);
}

public class AccessDomainService : IAccessDomainService
{
    private readonly IPlaneHistoryCacheRepository _planeCacheRepository;
    private readonly IPlaneHistoryRepository _planeHistoryRepository;
    private readonly ILastSeenPointerRepository _lastSeenPointerRepository;

    public AccessDomainService(
        IPlaneHistoryCacheRepository planeCacheRepository,
        IPlaneHistoryRepository planeHistoryRepository,
        ILastSeenPointerRepository lastSeenPointerRepository)
    {
        _planeCacheRepository = planeCacheRepository;
        _lastSeenPointerRepository = lastSeenPointerRepository;
        _planeHistoryRepository = planeHistoryRepository;
    }

    public async Task<PlaneDataRecordLink> RetrievePlaneHistory(string hexValue, long time)
    {
        if(time == 0)
        {
            return new();
        }
        var now = DateTime.UtcNow;

        var lastMinuteSecond = MinuteAlignedTimestamp(time);
        // if you are early (before latest minute)  then get from redis
        // if yu are on time (between -1m and -2m) then find out what the last seen is.
        //  If the last seen is -1m, then you already have the info
        //  If the last seen is -2m or more
        if(time > lastMinuteSecond)
        {
            return await GetPlaneDataFromRedis(hexValue,lastMinuteSecond, time, time - 60);
        }
        if(time == lastMinuteSecond-60)
        {
            //get last seen
            var lastSeen = await _lastSeenPointerRepository.GetLastSeenTimeAsync(hexValue);
            
            if(lastSeen == time)
            {
                //we are here because we have already stored this info
                return await _planeHistoryRepository.GetPlaneHistory(hexValue,time);
            }
            //we have not recoreded this minute yet, fetch from redis
            //
            return await GetPlaneDataFromRedis(hexValue,time,time+59, lastSeen);
        }
        
        var result = await _planeHistoryRepository.GetPlaneHistory(hexValue,time);
        
        if(result.PreviousLink == 0 && !result.Planes.Any())
        {
            result.PreviousLink = await _lastSeenPointerRepository.GetLastSeenTimeAsync(hexValue);
        }
        
        return result;
    }
    
    private async Task<PlaneDataRecordLink> GetPlaneDataFromRedis(
        string hexValue,
        long startTime, 
        long endTime, 
        long previousLink) => new()
        {
            Planes = await _planeCacheRepository.GetPlanesInRange(startTime,endTime,hexValue).ToListAsync(),
            PreviousLink = previousLink
        };

    
    private long MinuteAlignedTimestamp(long time)
    {
        var offsetTime = DateTime.UtcNow.AddSeconds(time);
        
        var minuteAlignedTime = offsetTime.AddSeconds(0 - offsetTime.Second);
        
        return (long)minuteAlignedTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}