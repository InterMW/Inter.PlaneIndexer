using Application.Messages;
using DomainService;
using MelbergFramework.Infrastructure.Rabbit.Consumers;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Rabbit.Translator;

namespace Application.Pillars;

public class IngressProcessor : IStandardConsumer
{
    private readonly IIngressDomainService _service;
    private readonly IJsonToObjectTranslator<CompletedPlaneFrameMessage> _translator;
    public IngressProcessor(IIngressDomainService service,IJsonToObjectTranslator<CompletedPlaneFrameMessage> translator)
    {
        _service = service;
        _translator = translator;
    }
    public async Task ConsumeMessageAsync(Message message, CancellationToken ct)
    {
        var completedPlaneSet = _translator.Translate(message);
        
        await _service.RecordPlanes(completedPlaneSet.Planes, completedPlaneSet.Now);
    } 
}
