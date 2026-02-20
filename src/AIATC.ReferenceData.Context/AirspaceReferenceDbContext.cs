using System;
using System.Collections.Generic;
using AIATC.ReferenceData.Models;
using Microsoft.EntityFrameworkCore;

namespace AIATC.ReferenceData.Context;

public partial class AirspaceReferenceDbContext : DbContext
{
    public AirspaceReferenceDbContext(DbContextOptions<AirspaceReferenceDbContext> options)
        : base(options)
    {
        // Make context read-only
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<Approach> Approaches { get; set; }

    public virtual DbSet<ApproachCont> ApproachConts { get; set; }

    public virtual DbSet<ControlledAirspace> ControlledAirspaces { get; set; }

    public virtual DbSet<EnrouteAirway> EnrouteAirways { get; set; }

    public virtual DbSet<EnrouteWaypoint> EnrouteWaypoints { get; set; }

    public virtual DbSet<GridMora> GridMoras { get; set; }

    public virtual DbSet<HeliApproach> HeliApproaches { get; set; }

    public virtual DbSet<HeliApproachCont> HeliApproachConts { get; set; }

    public virtual DbSet<HeliMsa> HeliMsas { get; set; }

    public virtual DbSet<HeliTerminalWaypoint> HeliTerminalWaypoints { get; set; }

    public virtual DbSet<Heliport> Heliports { get; set; }

    public virtual DbSet<Localizer> Localizers { get; set; }

    public virtual DbSet<Msa> Msas { get; set; }

    public virtual DbSet<NdbNavaid> NdbNavaids { get; set; }

    public virtual DbSet<Pathpoint> Pathpoints { get; set; }

    public virtual DbSet<PathpointCont> PathpointConts { get; set; }

    public virtual DbSet<RestrictiveAirspace> RestrictiveAirspaces { get; set; }

    public virtual DbSet<RestrictiveAirspaceCont> RestrictiveAirspaceConts { get; set; }

    public virtual DbSet<Runway> Runways { get; set; }

    public virtual DbSet<Sid> Sids { get; set; }

    public virtual DbSet<Star> Stars { get; set; }

    public virtual DbSet<TerminalNavaid> TerminalNavaids { get; set; }

    public virtual DbSet<TerminalWaypoint> TerminalWaypoints { get; set; }

    public virtual DbSet<VhfNavaid> VhfNavaids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Airport>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("airport", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.AirportName)
                .HasColumnType("character varying")
                .HasColumnName("airport_name");
            entity.Property(e => e.AtaIataDesignator)
                .HasColumnType("character varying")
                .HasColumnName("ata__iata_designator");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DaylightIndicator)
                .HasColumnType("character varying")
                .HasColumnName("daylight_indicator");
            entity.Property(e => e.Elevation)
                .HasColumnType("character varying")
                .HasColumnName("elevation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Ifr)
                .HasColumnType("character varying")
                .HasColumnName("ifr");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.LongestRunway)
                .HasColumnType("character varying")
                .HasColumnName("longest_runway");
            entity.Property(e => e.LongestRunwaySurfaceCode)
                .HasColumnType("character varying")
                .HasColumnName("longest_runway_surface_code");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.MagneticTrueIndicator)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_true_indicator");
            entity.Property(e => e.MagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_variation");
            entity.Property(e => e.PublicMilitaryIndicator)
                .HasColumnType("character varying")
                .HasColumnName("public_military_indicator");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitAltitude)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_altitude");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.TimeZone)
                .HasColumnType("character varying")
                .HasColumnName("time_zone");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionLevel)
                .HasColumnType("character varying")
                .HasColumnName("transition_level");
        });

        modelBuilder.Entity<Approach>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("approach", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.Altitude1)
                .HasColumnType("character varying")
                .HasColumnName("altitude_1");
            entity.Property(e => e.Altitude2)
                .HasColumnType("character varying")
                .HasColumnName("altitude_2");
            entity.Property(e => e.AltitudeDescriptor)
                .HasColumnType("character varying")
                .HasColumnName("altitude_descriptor");
            entity.Property(e => e.ArcRadius)
                .HasColumnType("character varying")
                .HasColumnName("arc_radius");
            entity.Property(e => e.AtcIndicator)
                .HasColumnType("character varying")
                .HasColumnName("atc_indicator");
            entity.Property(e => e.CenterFix)
                .HasColumnType("character varying")
                .HasColumnName("center_fix");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DescriptionCode)
                .HasColumnType("character varying")
                .HasColumnName("description_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.GnssFmsIndicator)
                .HasColumnType("character varying")
                .HasColumnName("gnss__fms_indicator");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.IcaoCode3)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_3");
            entity.Property(e => e.IcaoCode4)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_4");
            entity.Property(e => e.MagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_course");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.PathTerminator)
                .HasColumnType("character varying")
                .HasColumnName("path_terminator");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rho)
                .HasColumnType("character varying")
                .HasColumnName("rho");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.RouteHoldDistanceTime)
                .HasColumnType("character varying")
                .HasColumnName("route_hold_distance_time");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("section_code_3");
            entity.Property(e => e.SectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("section_code_4");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_indicator");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.SubsectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_3");
            entity.Property(e => e.SubsectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_4");
            entity.Property(e => e.Theta)
                .HasColumnType("character varying")
                .HasColumnName("theta");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
            entity.Property(e => e.TurnDirection)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction");
            entity.Property(e => e.TurnDirectionValid)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction_valid");
            entity.Property(e => e.VerticalAngle)
                .HasColumnType("character varying")
                .HasColumnName("vertical_angle");
        });

        modelBuilder.Entity<ApproachCont>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("approach_cont", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ApplicationType)
                .HasColumnType("character varying")
                .HasColumnName("application_type");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FasBlock)
                .HasColumnType("character varying")
                .HasColumnName("fas_block");
            entity.Property(e => e.FasBlockLosName)
                .HasColumnType("character varying")
                .HasColumnName("fas_block_los_name");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Lnav)
                .HasColumnType("character varying")
                .HasColumnName("lnav");
            entity.Property(e => e.LnavLosName)
                .HasColumnType("character varying")
                .HasColumnName("lnav_los_name");
            entity.Property(e => e.LnavVnav)
                .HasColumnType("character varying")
                .HasColumnName("lnav__vnav");
            entity.Property(e => e.LnavVnavLosName)
                .HasColumnType("character varying")
                .HasColumnName("lnav__vnav_los_name");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
        });

        modelBuilder.Entity<ControlledAirspace>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("controlled_airspace", "cycle2508");

            entity.Property(e => e.AirspaceCenter)
                .HasColumnType("character varying")
                .HasColumnName("airspace_center");
            entity.Property(e => e.AirspaceClass)
                .HasColumnType("character varying")
                .HasColumnName("airspace_class");
            entity.Property(e => e.AirspaceName)
                .HasColumnType("character varying")
                .HasColumnName("airspace_name");
            entity.Property(e => e.AirspaceType)
                .HasColumnType("character varying")
                .HasColumnName("airspace_type");
            entity.Property(e => e.ArcBearing)
                .HasColumnType("character varying")
                .HasColumnName("arc_bearing");
            entity.Property(e => e.ArcDistance)
                .HasColumnType("character varying")
                .HasColumnName("arc_distance");
            entity.Property(e => e.ArcOriginLatitude)
                .HasColumnType("character varying")
                .HasColumnName("arc_origin_latitude");
            entity.Property(e => e.ArcOriginLongitude)
                .HasColumnType("character varying")
                .HasColumnName("arc_origin_longitude");
            entity.Property(e => e.BoundaryVia)
                .HasColumnType("character varying")
                .HasColumnName("boundary_via");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Level)
                .HasColumnType("character varying")
                .HasColumnName("level");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.LowerLimit)
                .HasColumnType("character varying")
                .HasColumnName("lower_limit");
            entity.Property(e => e.LowerLimitUnitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("lower_limit_unit_indicator");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.Notam)
                .HasColumnType("character varying")
                .HasColumnName("notam");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.TimeCode)
                .HasColumnType("character varying")
                .HasColumnName("time_code");
            entity.Property(e => e.UpperLimit)
                .HasColumnType("character varying")
                .HasColumnName("upper_limit");
            entity.Property(e => e.UpperLimitUnitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("upper_limit_unit_indicator");
        });

        modelBuilder.Entity<EnrouteAirway>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("enroute_airways", "cycle2508");

            entity.Property(e => e.BoundaryCode)
                .HasColumnType("character varying")
                .HasColumnName("boundary_code");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CruiseTableIndicator)
                .HasColumnType("character varying")
                .HasColumnName("cruise_table_indicator");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DirectionRestriction)
                .HasColumnType("character varying")
                .HasColumnName("direction_restriction");
            entity.Property(e => e.EuIndicator)
                .HasColumnType("character varying")
                .HasColumnName("eu_indicator");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.InboundMagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("inbound_magnetic_course");
            entity.Property(e => e.Level)
                .HasColumnType("character varying")
                .HasColumnName("level");
            entity.Property(e => e.MaximumAltitude)
                .HasColumnType("character varying")
                .HasColumnName("maximum_altitude");
            entity.Property(e => e.MinimumAltitude1)
                .HasColumnType("character varying")
                .HasColumnName("minimum_altitude_1");
            entity.Property(e => e.MinimumAltitude2)
                .HasColumnType("character varying")
                .HasColumnName("minimum_altitude_2");
            entity.Property(e => e.OutboundMagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("outbound_magnetic_course");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rho)
                .HasColumnType("character varying")
                .HasColumnName("rho");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.RouteDistanceFrom)
                .HasColumnType("character varying")
                .HasColumnName("route_distance_from");
            entity.Property(e => e.RouteIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("route_identifier");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SixthCharacter)
                .HasColumnType("character varying")
                .HasColumnName("sixth_character");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.Theta)
                .HasColumnType("character varying")
                .HasColumnName("theta");
            entity.Property(e => e.WaypointDescriptionCode)
                .HasColumnType("character varying")
                .HasColumnName("waypoint_description_code");
        });

        modelBuilder.Entity<EnrouteWaypoint>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("enroute_waypoint", "cycle2508");

            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DynamicMagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("dynamic_magnetic_variation");
            entity.Property(e => e.Elevation)
                .HasColumnType("character varying")
                .HasColumnName("elevation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.NameDescription)
                .HasColumnType("character varying")
                .HasColumnName("name__description");
            entity.Property(e => e.NameFormatIndicator)
                .HasColumnType("character varying")
                .HasColumnName("name_format_indicator");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RegionCode)
                .HasColumnType("character varying")
                .HasColumnName("region_code");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
            entity.Property(e => e.Usage)
                .HasColumnType("character varying")
                .HasColumnName("usage");
            entity.Property(e => e.WaypointIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("waypoint_identifier");
        });

        modelBuilder.Entity<GridMora>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("grid_mora", "cycle2508");

            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.Mora1)
                .HasColumnType("character varying")
                .HasColumnName("mora_1");
            entity.Property(e => e.Mora10)
                .HasColumnType("character varying")
                .HasColumnName("mora_10");
            entity.Property(e => e.Mora11)
                .HasColumnType("character varying")
                .HasColumnName("mora_11");
            entity.Property(e => e.Mora12)
                .HasColumnType("character varying")
                .HasColumnName("mora_12");
            entity.Property(e => e.Mora13)
                .HasColumnType("character varying")
                .HasColumnName("mora_13");
            entity.Property(e => e.Mora14)
                .HasColumnType("character varying")
                .HasColumnName("mora_14");
            entity.Property(e => e.Mora15)
                .HasColumnType("character varying")
                .HasColumnName("mora_15");
            entity.Property(e => e.Mora16)
                .HasColumnType("character varying")
                .HasColumnName("mora_16");
            entity.Property(e => e.Mora17)
                .HasColumnType("character varying")
                .HasColumnName("mora_17");
            entity.Property(e => e.Mora18)
                .HasColumnType("character varying")
                .HasColumnName("mora_18");
            entity.Property(e => e.Mora19)
                .HasColumnType("character varying")
                .HasColumnName("mora_19");
            entity.Property(e => e.Mora2)
                .HasColumnType("character varying")
                .HasColumnName("mora_2");
            entity.Property(e => e.Mora20)
                .HasColumnType("character varying")
                .HasColumnName("mora_20");
            entity.Property(e => e.Mora21)
                .HasColumnType("character varying")
                .HasColumnName("mora_21");
            entity.Property(e => e.Mora22)
                .HasColumnType("character varying")
                .HasColumnName("mora_22");
            entity.Property(e => e.Mora23)
                .HasColumnType("character varying")
                .HasColumnName("mora_23");
            entity.Property(e => e.Mora24)
                .HasColumnType("character varying")
                .HasColumnName("mora_24");
            entity.Property(e => e.Mora25)
                .HasColumnType("character varying")
                .HasColumnName("mora_25");
            entity.Property(e => e.Mora26)
                .HasColumnType("character varying")
                .HasColumnName("mora_26");
            entity.Property(e => e.Mora27)
                .HasColumnType("character varying")
                .HasColumnName("mora_27");
            entity.Property(e => e.Mora28)
                .HasColumnType("character varying")
                .HasColumnName("mora_28");
            entity.Property(e => e.Mora29)
                .HasColumnType("character varying")
                .HasColumnName("mora_29");
            entity.Property(e => e.Mora3)
                .HasColumnType("character varying")
                .HasColumnName("mora_3");
            entity.Property(e => e.Mora30)
                .HasColumnType("character varying")
                .HasColumnName("mora_30");
            entity.Property(e => e.Mora4)
                .HasColumnType("character varying")
                .HasColumnName("mora_4");
            entity.Property(e => e.Mora5)
                .HasColumnType("character varying")
                .HasColumnName("mora_5");
            entity.Property(e => e.Mora6)
                .HasColumnType("character varying")
                .HasColumnName("mora_6");
            entity.Property(e => e.Mora7)
                .HasColumnType("character varying")
                .HasColumnName("mora_7");
            entity.Property(e => e.Mora8)
                .HasColumnType("character varying")
                .HasColumnName("mora_8");
            entity.Property(e => e.Mora9)
                .HasColumnType("character varying")
                .HasColumnName("mora_9");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.StartLatitude)
                .HasColumnType("character varying")
                .HasColumnName("start_latitude");
            entity.Property(e => e.StartLongitude)
                .HasColumnType("character varying")
                .HasColumnName("start_longitude");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
        });

        modelBuilder.Entity<HeliApproach>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("heli_approach", "cycle2508");

            entity.Property(e => e.Altitude1)
                .HasColumnType("character varying")
                .HasColumnName("altitude_1");
            entity.Property(e => e.Altitude2)
                .HasColumnType("character varying")
                .HasColumnName("altitude_2");
            entity.Property(e => e.AltitudeDescriptor)
                .HasColumnType("character varying")
                .HasColumnName("altitude_descriptor");
            entity.Property(e => e.ArcRadius)
                .HasColumnType("character varying")
                .HasColumnName("arc_radius");
            entity.Property(e => e.AtcIndicator)
                .HasColumnType("character varying")
                .HasColumnName("atc_indicator");
            entity.Property(e => e.CenterFix)
                .HasColumnType("character varying")
                .HasColumnName("center_fix");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DescriptionCode)
                .HasColumnType("character varying")
                .HasColumnName("description_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.GnssFmsIndicator)
                .HasColumnType("character varying")
                .HasColumnName("gnss__fms_indicator");
            entity.Property(e => e.HeliportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("heliport_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.IcaoCode3)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_3");
            entity.Property(e => e.IcaoCode4)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_4");
            entity.Property(e => e.MagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_course");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.PathTerminator)
                .HasColumnType("character varying")
                .HasColumnName("path_terminator");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rho)
                .HasColumnType("character varying")
                .HasColumnName("rho");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.RouteHoldDistanceTime)
                .HasColumnType("character varying")
                .HasColumnName("route_hold_distance_time");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("section_code_3");
            entity.Property(e => e.SectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("section_code_4");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_indicator");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.SubsectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_3");
            entity.Property(e => e.SubsectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_4");
            entity.Property(e => e.Theta)
                .HasColumnType("character varying")
                .HasColumnName("theta");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
            entity.Property(e => e.TurnDirection)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction");
            entity.Property(e => e.TurnDirectionValid)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction_valid");
            entity.Property(e => e.VerticalAngle)
                .HasColumnType("character varying")
                .HasColumnName("vertical_angle");
        });

        modelBuilder.Entity<HeliApproachCont>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("heli_approach_cont", "cycle2508");

            entity.Property(e => e.ApplicationType)
                .HasColumnType("character varying")
                .HasColumnName("application_type");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FasBlock)
                .HasColumnType("character varying")
                .HasColumnName("fas_block");
            entity.Property(e => e.FasBlockLosName)
                .HasColumnType("character varying")
                .HasColumnName("fas_block_los_name");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.HeliportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("heliport_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Lnav)
                .HasColumnType("character varying")
                .HasColumnName("lnav");
            entity.Property(e => e.LnavLosName)
                .HasColumnType("character varying")
                .HasColumnName("lnav_los_name");
            entity.Property(e => e.LnavVnav)
                .HasColumnType("character varying")
                .HasColumnName("lnav__vnav");
            entity.Property(e => e.LnavVnavLosName)
                .HasColumnType("character varying")
                .HasColumnName("lnav__vnav_los_name");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
        });

        modelBuilder.Entity<HeliMsa>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("heli_msa", "cycle2508");

            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.HeliportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("heliport_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.MagneticTrueIndicator)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_true_indicator");
            entity.Property(e => e.MsaCenter)
                .HasColumnType("character varying")
                .HasColumnName("msa_center");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectorAltitude1)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_1");
            entity.Property(e => e.SectorAltitude2)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_2");
            entity.Property(e => e.SectorAltitude3)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_3");
            entity.Property(e => e.SectorAltitude4)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_4");
            entity.Property(e => e.SectorAltitude5)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_5");
            entity.Property(e => e.SectorAltitude6)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_6");
            entity.Property(e => e.SectorAltitude7)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_7");
            entity.Property(e => e.SectorBearing1)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_1");
            entity.Property(e => e.SectorBearing2)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_2");
            entity.Property(e => e.SectorBearing3)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_3");
            entity.Property(e => e.SectorBearing4)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_4");
            entity.Property(e => e.SectorBearing5)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_5");
            entity.Property(e => e.SectorBearing6)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_6");
            entity.Property(e => e.SectorBearing7)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_7");
            entity.Property(e => e.SectorRadial1)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_1");
            entity.Property(e => e.SectorRadial2)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_2");
            entity.Property(e => e.SectorRadial3)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_3");
            entity.Property(e => e.SectorRadial4)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_4");
            entity.Property(e => e.SectorRadial5)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_5");
            entity.Property(e => e.SectorRadial6)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_6");
            entity.Property(e => e.SectorRadial7)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_7");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
        });

        modelBuilder.Entity<HeliTerminalWaypoint>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("heli_terminal_waypoint", "cycle2508");

            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DynamicMagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("dynamic_magnetic_variation");
            entity.Property(e => e.Elevation)
                .HasColumnType("character varying")
                .HasColumnName("elevation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.HeliportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("heliport_identifier");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.NameDescription)
                .HasColumnType("character varying")
                .HasColumnName("name__description");
            entity.Property(e => e.NameFormatIndicator)
                .HasColumnType("character varying")
                .HasColumnName("name_format_indicator");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
            entity.Property(e => e.Usage)
                .HasColumnType("character varying")
                .HasColumnName("usage");
            entity.Property(e => e.WaypointIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("waypoint_identifier");
        });

        modelBuilder.Entity<Heliport>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("heliport", "cycle2508");

            entity.Property(e => e.AtaIataDesignator)
                .HasColumnType("character varying")
                .HasColumnName("ata__iata_designator");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DaylightIndicator)
                .HasColumnType("character varying")
                .HasColumnName("daylight_indicator");
            entity.Property(e => e.Elevation)
                .HasColumnType("character varying")
                .HasColumnName("elevation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.HeliportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("heliport_identifier");
            entity.Property(e => e.HeliportName)
                .HasColumnType("character varying")
                .HasColumnName("heliport_name");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Ifr)
                .HasColumnType("character varying")
                .HasColumnName("ifr");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.MagneticTrueIndicator)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_true_indicator");
            entity.Property(e => e.MagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_variation");
            entity.Property(e => e.PadDimensions)
                .HasColumnType("character varying")
                .HasColumnName("pad_dimensions");
            entity.Property(e => e.PadIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("pad_identifier");
            entity.Property(e => e.PublicMilitaryIndicator)
                .HasColumnType("character varying")
                .HasColumnName("public_military_indicator");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitAltitude)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_altitude");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.TimeZone)
                .HasColumnType("character varying")
                .HasColumnName("time_zone");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionLevel)
                .HasColumnType("character varying")
                .HasColumnName("transition_level");
        });

        modelBuilder.Entity<Localizer>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("localizer", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.GlideSlopeAngle)
                .HasColumnType("character varying")
                .HasColumnName("glide_slope_angle");
            entity.Property(e => e.GlideSlopeElevation)
                .HasColumnType("character varying")
                .HasColumnName("glide_slope_elevation");
            entity.Property(e => e.GlideSlopeLatitude)
                .HasColumnType("character varying")
                .HasColumnName("glide_slope_latitude");
            entity.Property(e => e.GlideSlopeLongitude)
                .HasColumnType("character varying")
                .HasColumnName("glide_slope_longitude");
            entity.Property(e => e.GlideSlopePosition)
                .HasColumnType("character varying")
                .HasColumnName("glide_slope_position");
            entity.Property(e => e.GsThresholdLandingHeight)
                .HasColumnType("character varying")
                .HasColumnName("gs_threshold_landing_height");
            entity.Property(e => e.IlsCategory)
                .HasColumnType("character varying")
                .HasColumnName("ils_category");
            entity.Property(e => e.LocalizerBearing)
                .HasColumnType("character varying")
                .HasColumnName("localizer_bearing");
            entity.Property(e => e.LocalizerFrequency)
                .HasColumnType("character varying")
                .HasColumnName("localizer_frequency");
            entity.Property(e => e.LocalizerIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("localizer_identifier");
            entity.Property(e => e.LocalizerLatitude)
                .HasColumnType("character varying")
                .HasColumnName("localizer_latitude");
            entity.Property(e => e.LocalizerLongitude)
                .HasColumnType("character varying")
                .HasColumnName("localizer_longitude");
            entity.Property(e => e.LocalizerPosition)
                .HasColumnType("character varying")
                .HasColumnName("localizer_position");
            entity.Property(e => e.LocalizerPositionRef)
                .HasColumnType("character varying")
                .HasColumnName("localizer_position_ref");
            entity.Property(e => e.LocalizerWidth)
                .HasColumnType("character varying")
                .HasColumnName("localizer_width");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RunwayIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("runway_identifier");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.StationDeclination)
                .HasColumnType("character varying")
                .HasColumnName("station_declination");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
        });

        modelBuilder.Entity<Msa>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("msa", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.MagneticTrueIndicator)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_true_indicator");
            entity.Property(e => e.MsaCenter)
                .HasColumnType("character varying")
                .HasColumnName("msa_center");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectorAltitude1)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_1");
            entity.Property(e => e.SectorAltitude2)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_2");
            entity.Property(e => e.SectorAltitude3)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_3");
            entity.Property(e => e.SectorAltitude4)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_4");
            entity.Property(e => e.SectorAltitude5)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_5");
            entity.Property(e => e.SectorAltitude6)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_6");
            entity.Property(e => e.SectorAltitude7)
                .HasColumnType("character varying")
                .HasColumnName("sector_altitude_7");
            entity.Property(e => e.SectorBearing1)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_1");
            entity.Property(e => e.SectorBearing2)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_2");
            entity.Property(e => e.SectorBearing3)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_3");
            entity.Property(e => e.SectorBearing4)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_4");
            entity.Property(e => e.SectorBearing5)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_5");
            entity.Property(e => e.SectorBearing6)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_6");
            entity.Property(e => e.SectorBearing7)
                .HasColumnType("character varying")
                .HasColumnName("sector_bearing_7");
            entity.Property(e => e.SectorRadial1)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_1");
            entity.Property(e => e.SectorRadial2)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_2");
            entity.Property(e => e.SectorRadial3)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_3");
            entity.Property(e => e.SectorRadial4)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_4");
            entity.Property(e => e.SectorRadial5)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_5");
            entity.Property(e => e.SectorRadial6)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_6");
            entity.Property(e => e.SectorRadial7)
                .HasColumnType("character varying")
                .HasColumnName("sector_radial_7");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
        });

        modelBuilder.Entity<NdbNavaid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ndb_navaid", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.MagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_variation");
            entity.Property(e => e.NdbClass)
                .HasColumnType("character varying")
                .HasColumnName("ndb_class");
            entity.Property(e => e.NdbFrequency)
                .HasColumnType("character varying")
                .HasColumnName("ndb_frequency");
            entity.Property(e => e.NdbIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("ndb_identifier");
            entity.Property(e => e.NdbLatitude)
                .HasColumnType("character varying")
                .HasColumnName("ndb_latitude");
            entity.Property(e => e.NdbLongitude)
                .HasColumnType("character varying")
                .HasColumnName("ndb_longitude");
            entity.Property(e => e.NdbName)
                .HasColumnType("character varying")
                .HasColumnName("ndb_name");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
        });

        modelBuilder.Entity<Pathpoint>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("pathpoint", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ApproachIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("approach_identifier");
            entity.Property(e => e.ApproachPerformanceDesignator)
                .HasColumnType("character varying")
                .HasColumnName("approach_performance_designator");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CourseWidthAtThreshold)
                .HasColumnType("character varying")
                .HasColumnName("course_width_at_threshold");
            entity.Property(e => e.CrcRemainder)
                .HasColumnType("character varying")
                .HasColumnName("crc_remainder");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FpapLatitude)
                .HasColumnType("character varying")
                .HasColumnName("fpap_latitude");
            entity.Property(e => e.FpapLongitude)
                .HasColumnType("character varying")
                .HasColumnName("fpap_longitude");
            entity.Property(e => e.Gpa)
                .HasColumnType("character varying")
                .HasColumnName("gpa");
            entity.Property(e => e.Hal)
                .HasColumnType("character varying")
                .HasColumnName("hal");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.LengthOffset)
                .HasColumnType("character varying")
                .HasColumnName("length_offset");
            entity.Property(e => e.LtpEllipsoidHeight)
                .HasColumnType("character varying")
                .HasColumnName("ltp_ellipsoid_height");
            entity.Property(e => e.LtpLatitude)
                .HasColumnType("character varying")
                .HasColumnName("ltp_latitude");
            entity.Property(e => e.LtpLongitude)
                .HasColumnType("character varying")
                .HasColumnName("ltp_longitude");
            entity.Property(e => e.OperationsType)
                .HasColumnType("character varying")
                .HasColumnName("operations_type");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.ReferencePathDataIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("reference_path_data_identifier");
            entity.Property(e => e.ReferencePathDataSelector)
                .HasColumnType("character varying")
                .HasColumnName("reference_path_data_selector");
            entity.Property(e => e.RouteIndicator)
                .HasColumnType("character varying")
                .HasColumnName("route_indicator");
            entity.Property(e => e.RunwayOrHelipadIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("runway_or_helipad_identifier");
            entity.Property(e => e.SbasServiceProvider)
                .HasColumnType("character varying")
                .HasColumnName("sbas_service_provider");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.Tch)
                .HasColumnType("character varying")
                .HasColumnName("tch");
            entity.Property(e => e.TchUnitsSelector)
                .HasColumnType("character varying")
                .HasColumnName("tch_units_selector");
            entity.Property(e => e.Val)
                .HasColumnType("character varying")
                .HasColumnName("val");
        });

        modelBuilder.Entity<PathpointCont>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("pathpoint_cont", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ApplicationType)
                .HasColumnType("character varying")
                .HasColumnName("application_type");
            entity.Property(e => e.ApproachIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("approach_identifier");
            entity.Property(e => e.ApproachTypeIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("approach_type_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FpapEllipsoidHeight)
                .HasColumnType("character varying")
                .HasColumnName("fpap_ellipsoid_height");
            entity.Property(e => e.FpapOrthometricHeight)
                .HasColumnType("character varying")
                .HasColumnName("fpap_orthometric_height");
            entity.Property(e => e.GnssChannelNumber)
                .HasColumnType("character varying")
                .HasColumnName("gnss_channel_number");
            entity.Property(e => e.Hpc)
                .HasColumnType("character varying")
                .HasColumnName("hpc");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.LtpOrthometricHeight)
                .HasColumnType("character varying")
                .HasColumnName("ltp_orthometric_height");
            entity.Property(e => e.OperationsType)
                .HasColumnType("character varying")
                .HasColumnName("operations_type");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Reserved)
                .HasColumnType("character varying")
                .HasColumnName("reserved");
            entity.Property(e => e.RunwayOrHelipadIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("runway_or_helipad_identifier");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
        });

        modelBuilder.Entity<RestrictiveAirspace>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("restrictive_airspace", "cycle2508");

            entity.Property(e => e.ArcBearing)
                .HasColumnType("character varying")
                .HasColumnName("arc_bearing");
            entity.Property(e => e.ArcDistance)
                .HasColumnType("character varying")
                .HasColumnName("arc_distance");
            entity.Property(e => e.ArcOriginLatitude)
                .HasColumnType("character varying")
                .HasColumnName("arc_origin_latitude");
            entity.Property(e => e.ArcOriginLongitude)
                .HasColumnType("character varying")
                .HasColumnName("arc_origin_longitude");
            entity.Property(e => e.BoundaryVia)
                .HasColumnType("character varying")
                .HasColumnName("boundary_via");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.Designation)
                .HasColumnType("character varying")
                .HasColumnName("designation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Level)
                .HasColumnType("character varying")
                .HasColumnName("level");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.LowerLimit)
                .HasColumnType("character varying")
                .HasColumnName("lower_limit");
            entity.Property(e => e.LowerLimitUnitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("lower_limit_unit_indicator");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.Notam)
                .HasColumnType("character varying")
                .HasColumnName("notam");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RestrictedAirspaceName)
                .HasColumnType("character varying")
                .HasColumnName("restricted_airspace_name");
            entity.Property(e => e.RestrictionType)
                .HasColumnType("character varying")
                .HasColumnName("restriction_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.TimeCode)
                .HasColumnType("character varying")
                .HasColumnName("time_code");
            entity.Property(e => e.UpperLimit)
                .HasColumnType("character varying")
                .HasColumnName("upper_limit");
            entity.Property(e => e.UpperLimitUnitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("upper_limit_unit_indicator");
        });

        modelBuilder.Entity<RestrictiveAirspaceCont>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("restrictive_airspace_cont", "cycle2508");

            entity.Property(e => e.ApplicationType)
                .HasColumnType("character varying")
                .HasColumnName("application_type");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.ControllingAgency)
                .HasColumnType("character varying")
                .HasColumnName("controlling_agency");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.Designation)
                .HasColumnType("character varying")
                .HasColumnName("designation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.Notam)
                .HasColumnType("character varying")
                .HasColumnName("notam");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RestrictionType)
                .HasColumnType("character varying")
                .HasColumnName("restriction_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.TimeCode)
                .HasColumnType("character varying")
                .HasColumnName("time_code");
            entity.Property(e => e.TimeIndicator)
                .HasColumnType("character varying")
                .HasColumnName("time_indicator");
            entity.Property(e => e.TimeOfOperation1)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_1");
            entity.Property(e => e.TimeOfOperation2)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_2");
            entity.Property(e => e.TimeOfOperation3)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_3");
            entity.Property(e => e.TimeOfOperation4)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_4");
            entity.Property(e => e.TimeOfOperation5)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_5");
            entity.Property(e => e.TimeOfOperation6)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_6");
            entity.Property(e => e.TimeOfOperation7)
                .HasColumnType("character varying")
                .HasColumnName("time_of_operation_7");
        });

        modelBuilder.Entity<Runway>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("runway", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.CategoryClass)
                .HasColumnType("character varying")
                .HasColumnName("category__class");
            entity.Property(e => e.CategoryClass2)
                .HasColumnType("character varying")
                .HasColumnName("category__class_2");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DisplacedThreshold)
                .HasColumnType("character varying")
                .HasColumnName("displaced_threshold");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.LandingThresholdElevation)
                .HasColumnType("character varying")
                .HasColumnName("landing_threshold_elevation");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.LocMlsGlsIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("loc__mls__gls_identifier");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.LtpElipsoidHeight)
                .HasColumnType("character varying")
                .HasColumnName("ltp_elipsoid_height");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.RunwayBearing)
                .HasColumnType("character varying")
                .HasColumnName("runway_bearing");
            entity.Property(e => e.RunwayDescription)
                .HasColumnType("character varying")
                .HasColumnName("runway_description");
            entity.Property(e => e.RunwayGradient)
                .HasColumnType("character varying")
                .HasColumnName("runway_gradient");
            entity.Property(e => e.RunwayIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("runway_identifier");
            entity.Property(e => e.RunwayLength)
                .HasColumnType("character varying")
                .HasColumnName("runway_length");
            entity.Property(e => e.SecondaryLocMlsGlsIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("secondary_loc_mls_gls_identifier");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.Stopway)
                .HasColumnType("character varying")
                .HasColumnName("stopway");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.Tch)
                .HasColumnType("character varying")
                .HasColumnName("tch");
            entity.Property(e => e.TchValueIndicator)
                .HasColumnType("character varying")
                .HasColumnName("tch_value_indicator");
            entity.Property(e => e.Width)
                .HasColumnType("character varying")
                .HasColumnName("width");
        });

        modelBuilder.Entity<Sid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("sid", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.Altitude1)
                .HasColumnType("character varying")
                .HasColumnName("altitude_1");
            entity.Property(e => e.Altitude2)
                .HasColumnType("character varying")
                .HasColumnName("altitude_2");
            entity.Property(e => e.AltitudeDescriptor)
                .HasColumnType("character varying")
                .HasColumnName("altitude_descriptor");
            entity.Property(e => e.ArcRadius)
                .HasColumnType("character varying")
                .HasColumnName("arc_radius");
            entity.Property(e => e.AtcIndicator)
                .HasColumnType("character varying")
                .HasColumnName("atc_indicator");
            entity.Property(e => e.CenterFix)
                .HasColumnType("character varying")
                .HasColumnName("center_fix");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DescriptionCode)
                .HasColumnType("character varying")
                .HasColumnName("description_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.GnssFmsIndicator)
                .HasColumnType("character varying")
                .HasColumnName("gnss__fms_indicator");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.IcaoCode3)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_3");
            entity.Property(e => e.IcaoCode4)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_4");
            entity.Property(e => e.MagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_course");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.PathTerminator)
                .HasColumnType("character varying")
                .HasColumnName("path_terminator");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rho)
                .HasColumnType("character varying")
                .HasColumnName("rho");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.RouteHoldDistanceTime)
                .HasColumnType("character varying")
                .HasColumnName("route_hold_distance_time");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("section_code_3");
            entity.Property(e => e.SectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("section_code_4");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_indicator");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.SubsectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_3");
            entity.Property(e => e.SubsectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_4");
            entity.Property(e => e.Theta)
                .HasColumnType("character varying")
                .HasColumnName("theta");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
            entity.Property(e => e.TurnDirection)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction");
            entity.Property(e => e.TurnDirectionValid)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction_valid");
            entity.Property(e => e.VerticalAngle)
                .HasColumnType("character varying")
                .HasColumnName("vertical_angle");
        });

        modelBuilder.Entity<Star>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("star", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.Altitude1)
                .HasColumnType("character varying")
                .HasColumnName("altitude_1");
            entity.Property(e => e.Altitude2)
                .HasColumnType("character varying")
                .HasColumnName("altitude_2");
            entity.Property(e => e.AltitudeDescriptor)
                .HasColumnType("character varying")
                .HasColumnName("altitude_descriptor");
            entity.Property(e => e.ArcRadius)
                .HasColumnType("character varying")
                .HasColumnName("arc_radius");
            entity.Property(e => e.AtcIndicator)
                .HasColumnType("character varying")
                .HasColumnName("atc_indicator");
            entity.Property(e => e.CenterFix)
                .HasColumnType("character varying")
                .HasColumnName("center_fix");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DescriptionCode)
                .HasColumnType("character varying")
                .HasColumnName("description_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.FixIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("fix_identifier");
            entity.Property(e => e.GnssFmsIndicator)
                .HasColumnType("character varying")
                .HasColumnName("gnss__fms_indicator");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.IcaoCode3)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_3");
            entity.Property(e => e.IcaoCode4)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_4");
            entity.Property(e => e.MagneticCourse)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_course");
            entity.Property(e => e.MultipleCode)
                .HasColumnType("character varying")
                .HasColumnName("multiple_code");
            entity.Property(e => e.PathTerminator)
                .HasColumnType("character varying")
                .HasColumnName("path_terminator");
            entity.Property(e => e.ProcedureIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("procedure_identifier");
            entity.Property(e => e.RecommendedVhfNavaid)
                .HasColumnType("character varying")
                .HasColumnName("recommended_vhf_navaid");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.Rho)
                .HasColumnType("character varying")
                .HasColumnName("rho");
            entity.Property(e => e.Rnp)
                .HasColumnType("character varying")
                .HasColumnName("rnp");
            entity.Property(e => e.RouteHoldDistanceTime)
                .HasColumnType("character varying")
                .HasColumnName("route_hold_distance_time");
            entity.Property(e => e.RouteQualifier1)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_1");
            entity.Property(e => e.RouteQualifier2)
                .HasColumnType("character varying")
                .HasColumnName("route_qualifier_2");
            entity.Property(e => e.RouteType)
                .HasColumnType("character varying")
                .HasColumnName("route_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("section_code_2");
            entity.Property(e => e.SectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("section_code_3");
            entity.Property(e => e.SectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("section_code_4");
            entity.Property(e => e.SequenceNumber)
                .HasColumnType("character varying")
                .HasColumnName("sequence_number");
            entity.Property(e => e.SpeedLimit)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit");
            entity.Property(e => e.SpeedLimitIndicator)
                .HasColumnType("character varying")
                .HasColumnName("speed_limit_indicator");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.SubsectionCode2)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_2");
            entity.Property(e => e.SubsectionCode3)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_3");
            entity.Property(e => e.SubsectionCode4)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code_4");
            entity.Property(e => e.Theta)
                .HasColumnType("character varying")
                .HasColumnName("theta");
            entity.Property(e => e.TransitionAltitude)
                .HasColumnType("character varying")
                .HasColumnName("transition_altitude");
            entity.Property(e => e.TransitionIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("transition_identifier");
            entity.Property(e => e.TurnDirection)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction");
            entity.Property(e => e.TurnDirectionValid)
                .HasColumnType("character varying")
                .HasColumnName("turn_direction_valid");
            entity.Property(e => e.VerticalAngle)
                .HasColumnType("character varying")
                .HasColumnName("vertical_angle");
        });

        modelBuilder.Entity<TerminalNavaid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("terminal_navaid", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.MagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("magnetic_variation");
            entity.Property(e => e.NdbClass)
                .HasColumnType("character varying")
                .HasColumnName("ndb_class");
            entity.Property(e => e.NdbFrequency)
                .HasColumnType("character varying")
                .HasColumnName("ndb_frequency");
            entity.Property(e => e.NdbIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("ndb_identifier");
            entity.Property(e => e.NdbLatitude)
                .HasColumnType("character varying")
                .HasColumnName("ndb_latitude");
            entity.Property(e => e.NdbLongitude)
                .HasColumnType("character varying")
                .HasColumnName("ndb_longitude");
            entity.Property(e => e.NdbName)
                .HasColumnType("character varying")
                .HasColumnName("ndb_name");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
        });

        modelBuilder.Entity<TerminalWaypoint>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("terminal_waypoint", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DynamicMagneticVariation)
                .HasColumnType("character varying")
                .HasColumnName("dynamic_magnetic_variation");
            entity.Property(e => e.Elevation)
                .HasColumnType("character varying")
                .HasColumnName("elevation");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.Latitude)
                .HasColumnType("character varying")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("character varying")
                .HasColumnName("longitude");
            entity.Property(e => e.NameDescription)
                .HasColumnType("character varying")
                .HasColumnName("name__description");
            entity.Property(e => e.NameFormatIndicator)
                .HasColumnType("character varying")
                .HasColumnName("name_format_indicator");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.Type)
                .HasColumnType("character varying")
                .HasColumnName("type");
            entity.Property(e => e.Usage)
                .HasColumnType("character varying")
                .HasColumnName("usage");
            entity.Property(e => e.WaypointIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("waypoint_identifier");
        });

        modelBuilder.Entity<VhfNavaid>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("vhf_navaid", "cycle2508");

            entity.Property(e => e.AirportIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("airport_identifier");
            entity.Property(e => e.ContinuationRecord)
                .HasColumnType("character varying")
                .HasColumnName("continuation_record");
            entity.Property(e => e.CustomerAreaCode)
                .HasColumnType("character varying")
                .HasColumnName("customer_area_code");
            entity.Property(e => e.Cycle)
                .HasColumnType("character varying")
                .HasColumnName("cycle");
            entity.Property(e => e.DatumCode)
                .HasColumnType("character varying")
                .HasColumnName("datum_code");
            entity.Property(e => e.DmeElevation)
                .HasColumnType("character varying")
                .HasColumnName("dme_elevation");
            entity.Property(e => e.DmeIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("dme_identifier");
            entity.Property(e => e.DmeLatitude)
                .HasColumnType("character varying")
                .HasColumnName("dme_latitude");
            entity.Property(e => e.DmeLongitude)
                .HasColumnType("character varying")
                .HasColumnName("dme_longitude");
            entity.Property(e => e.FileRecordNumber)
                .HasColumnType("character varying")
                .HasColumnName("file_record_number");
            entity.Property(e => e.Fom)
                .HasColumnType("character varying")
                .HasColumnName("fom");
            entity.Property(e => e.FrequencyProtection)
                .HasColumnType("character varying")
                .HasColumnName("frequency_protection");
            entity.Property(e => e.IcaoCode)
                .HasColumnType("character varying")
                .HasColumnName("icao_code");
            entity.Property(e => e.IcaoCode2)
                .HasColumnType("character varying")
                .HasColumnName("icao_code_2");
            entity.Property(e => e.IlsDmeBias)
                .HasColumnType("character varying")
                .HasColumnName("ils_dme_bias");
            entity.Property(e => e.NavaidClass)
                .HasColumnType("character varying")
                .HasColumnName("navaid_class");
            entity.Property(e => e.RecordType)
                .HasColumnType("character varying")
                .HasColumnName("record_type");
            entity.Property(e => e.SectionCode)
                .HasColumnType("character varying")
                .HasColumnName("section_code");
            entity.Property(e => e.StationDeclination)
                .HasColumnType("character varying")
                .HasColumnName("station_declination");
            entity.Property(e => e.SubsectionCode)
                .HasColumnType("character varying")
                .HasColumnName("subsection_code");
            entity.Property(e => e.VorFrequency)
                .HasColumnType("character varying")
                .HasColumnName("vor_frequency");
            entity.Property(e => e.VorIdentifier)
                .HasColumnType("character varying")
                .HasColumnName("vor_identifier");
            entity.Property(e => e.VorLatitude)
                .HasColumnType("character varying")
                .HasColumnName("vor_latitude");
            entity.Property(e => e.VorLongitude)
                .HasColumnType("character varying")
                .HasColumnName("vor_longitude");
            entity.Property(e => e.VorName)
                .HasColumnType("character varying")
                .HasColumnName("vor_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
