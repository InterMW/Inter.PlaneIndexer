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
    public Task<PlaneDataRecordLink> GetHistoryForPlane([FromQuery] string hexValue, [FromQuery] long? time) =>
        _service.RetrievePlaneHistory(hexValue.ToLower(),time ?? (long)(_clock.GetUtcNow() - DateTime.UnixEpoch).TotalSeconds);
}