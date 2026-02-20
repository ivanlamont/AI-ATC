namespace AIATC.Data.Models.Procedures;

using AIATC.Data.Models.Types;
using Ground;

/**<summary>
Multiple <c>Airport</c> and <c>Heliport STAR</c>
primary record sequences under same identifier.
</summary>
<remarks>See section 4.1.9.1 and 4.2.3.1.</remarks>*/
public class Arrival : Procedure<ArrivalSequence, ArrivalPoint>
{
    public int Id { get; set; }
}

/**<summary>
Multiple <c>Airport</c> and <c>Heliport Approach</c>
primary record sequences under same identifier.
</summary>
<remarks>See section 4.1.9.1 and 4.2.3.1.</remarks>*/
public class Approach : Procedure<ApproachSequence, ApproachPoint>
{
    public int Id { get; set; }

    public GroundPoint[]? GroundPoints { get; set; }

    public SatellitePoint[]? SatellitePoints { get; set; }
}

/**<summary>
Multiple <c>Airport</c> and <c>Heliport SID</c>
primary record sequences under same identifier.
</summary>
<remarks>See section 4.1.9.1 and 4.2.3.1.</remarks>*/
public class Departure : Procedure<DepartureSequence, DeparturePoint>
{
    public int Id { get; set; }
}
