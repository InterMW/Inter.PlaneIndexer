using Domain;
using DomainService;
using MelbergFramework.Core.Time;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexerController
{
    private readonly IAccessDomainService _service;
    private readonly IClock _clock;
    
    public IndexerController(
        IAccessDomainService service,
        IClock clock)
    {
        _service = service;
        _clock = clock;
    }
    
    [HttpGet]
    [Route("history")]
    public async Task<PlaneDataRecordLink> GetHistoryForPlane([FromQuery] string hexValue, [FromQuery] long? time)
    {
        var tim = time ?? ExtractTimestamp(_clock.GetUtcNow());
        var result = await _service.RetrievePlaneHistory(hexValue.ToUpper(), tim);//time ?? (long)(_clock.GetUtcNow() - DateTime.UnixEpoch).TotalSeconds);

        return result;
    }
    
    private long ExtractTimestamp(DateTime time) => 
        (long) Math.Floor(
            time.Subtract(DateTime.UnixEpoch).TotalSeconds
            );
}
