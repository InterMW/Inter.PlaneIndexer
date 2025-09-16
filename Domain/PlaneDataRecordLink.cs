namespace Domain;

public class PlaneDataRecordLink
{
    public string Hex;
    public long Time;
    public IEnumerable<PlaneMinimal> Planes {get; set;} = Array.Empty<PlaneMinimal>();
    public long? PreviousLink {get; set;} = 0;
}
