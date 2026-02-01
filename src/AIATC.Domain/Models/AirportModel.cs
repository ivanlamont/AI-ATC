namespace AIATC.Domain.Models;

/// <summary>
/// Represents an airport in the simulation.
/// Position is in nautical miles (NM) relative to scenario origin.
/// </summary>
public class AirportModel
{
    public string IcaoCode { get; set; } = string.Empty;
    public Vector2 PositionNm { get; set; }
    public float AltitudeFt { get; set; }
    public string Name { get; set; } = string.Empty;

    public AirportModel()
    {
        PositionNm = new Vector2(0, 0);
        AltitudeFt = 0;
    }

    public AirportModel(string icaoCode, Vector2 positionNm, float altitudeFt, string name = "")
    {
        IcaoCode = icaoCode;
        PositionNm = positionNm;
        AltitudeFt = altitudeFt;
        Name = name;
    }

    public float DistanceToNm(Vector2 position)
    {
        return Vector2.Distance(PositionNm, position);
    }
}
