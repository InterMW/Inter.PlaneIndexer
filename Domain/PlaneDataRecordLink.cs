namespace Domain;

public class PlaneDataRecordLink
{
    public IEnumerable<PlaneMinimal> Planes {get; set;} = Array.Empty<PlaneMinimal>();
    public long? PreviousLink {get; set;} = 0;
}