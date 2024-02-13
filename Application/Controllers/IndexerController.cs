using Domain;
using DomainService;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexerController
{
    private readonly IAccessDomainService _service;
    
    public IndexerController(IAccessDomainService service)
    {
        _service = service;
    }
    
    [HttpGet]
    [Route("history")]
    public Task<PlaneDataRecordLink> GetHistoryForPlane([FromQuery] string hexValue, [FromQuery] long? time) =>
        _service.RetrievePlaneHistory(hexValue,time ?? (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
}