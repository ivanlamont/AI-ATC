namespace AIATC.Data.Models;

using AIATC.Data.Models.Types;
using AIATC.Data.Types.Common;
using Ground;
using Routing;
using Waypoints;
using Navigation;

/**<summary>
Base object with an identifier that can be used as a navigation point.
</summary>*/
public abstract class Fix : Geo, IIcao, IIdentity
{
    public Icao Icao { get; set; }

    /**<summary>
    <para>
      <c>Fix Identifier (FIX IDENT)</c> field for
      <see cref="Waypoint"/> and <see cref="TerminalWaypoint"/>. See section 5.13.
    </para>
    <para>
      <c>Airport/Heliport Identifier (ARPT/HELI IDENT)</c> field for
      <see cref="Airport"/> and <see cref="Heliport"/>. See section 5.6.
    </para>
    <para>
      <c>VOR/NDB Identifier (VOR IDENT/NDB IDENT)</c> field for
      <see cref="Omnidirect"/>, <see cref="Nondirect"/> and <see cref="Tactical"/>. See section 5.33.
    </para>
    <para>
      <c>Localizer/MLS/GLS Identifier (LOC, MLS, GLS IDENT)</c> field for
      <see cref="InstrumentLanding"/>, <see cref="MicrowaveLanding"/>, <see cref="GlobalLanding"/>
      and <see cref="InstrumentMarker"/>. See section 5.44.
    </para>
    <para>
      <c>Gate Identifier (GATE IDENT)</c> field for
      <see cref="Gate"/>. See section 5.56.
    </para>
    <para>
      <c>Runway Identifier (RUNWAY ID)</c> field for
      <see cref="Threshold"/>. See section 5.46.
    </para>
    <para>
      <c>Marker Identifier (MARKER IDENT)</c> field for
      <see cref="AirwayMarker"/>. See section 5.110.
    </para>
    <para>
      <c>Reference Path Identifier (REF ID)</c> field for
      <see cref="SatellitePoint"/> and <see cref="GroundPoint"/>. See section 5.257.
    </para>
    <para>
      <c>Activity Identifier</c> field for
      <see cref="SpecialArea"/>. See section 5.279.
    </para>
    </summary>*/
    public string Identifier { get; set; }
}
