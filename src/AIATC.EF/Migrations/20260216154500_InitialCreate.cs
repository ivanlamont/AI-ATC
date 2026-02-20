using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AIATC.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    Tier = table.Column<string>(type: "text", nullable: true),
                    Criteria = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_types",
                columns: table => new
                {
                    IcaoCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    WakeCategory = table.Column<int>(type: "integer", nullable: false),
                    MaxTakeoffWeightLbs = table.Column<int>(type: "integer", nullable: false),
                    ServiceCeilingFt = table.Column<int>(type: "integer", nullable: false),
                    VrefSpeedKnots = table.Column<int>(type: "integer", nullable: false),
                    MinApproachSpeedKnots = table.Column<int>(type: "integer", nullable: false),
                    MaxCruiseSpeedKnots = table.Column<int>(type: "integer", nullable: false),
                    TypicalCruiseSpeedKnots = table.Column<int>(type: "integer", nullable: false),
                    MaxClimbRateFpm = table.Column<int>(type: "integer", nullable: false),
                    MaxDescentRateFpm = table.Column<int>(type: "integer", nullable: false),
                    StandardTurnRate = table.Column<float>(type: "real", nullable: false),
                    EngineType = table.Column<int>(type: "integer", nullable: false),
                    NumberOfEngines = table.Column<int>(type: "integer", nullable: false),
                    FuelConsumptionGph = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_types", x => x.IcaoCode);
                });

            migrationBuilder.CreateTable(
                name: "AirportCommunications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportCommunications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirwayCommunications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirwayCommunications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirwayPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    Descriptions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BoundaryCode = table.Column<byte>(type: "smallint", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    LevelType = table.Column<byte>(type: "smallint", nullable: false),
                    Restriction = table.Column<byte>(type: "smallint", nullable: false),
                    HasRestrictions = table.Column<byte>(type: "smallint", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    Theta = table.Column<float>(type: "real", nullable: false),
                    Rho = table.Column<float>(type: "real", nullable: false),
                    Out_Value = table.Column<float>(type: "real", nullable: false),
                    Out_Type = table.Column<byte>(type: "smallint", nullable: false),
                    DistanceFrom = table.Column<float>(type: "real", nullable: false),
                    In_Value = table.Column<float>(type: "real", nullable: false),
                    In_Type = table.Column<byte>(type: "smallint", nullable: false),
                    Minimum_Value = table.Column<float>(type: "real", nullable: false),
                    Minimum_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Minimum2_Value = table.Column<float>(type: "real", nullable: false),
                    Minimum2_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Maximum_Value = table.Column<float>(type: "real", nullable: false),
                    Maximum_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    FixRadius = table.Column<float>(type: "real", nullable: false),
                    ScaleFactor = table.Column<int>(type: "integer", nullable: false),
                    MinLevel = table.Column<int>(type: "integer", nullable: false),
                    MaxLevel = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirwayPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airways",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApproachPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VerticalAngle = table.Column<float>(type: "real", nullable: false),
                    Qualifiers = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    Descriptions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Turn = table.Column<byte>(type: "smallint", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    LegType = table.Column<byte>(type: "smallint", nullable: false),
                    IsTurnRequired = table.Column<byte>(type: "smallint", nullable: false),
                    ArcRadius = table.Column<float>(type: "real", nullable: false),
                    Theta = table.Column<float>(type: "real", nullable: false),
                    Rho = table.Column<float>(type: "real", nullable: false),
                    Course_Value = table.Column<float>(type: "real", nullable: false),
                    Course_Type = table.Column<byte>(type: "smallint", nullable: false),
                    DistanceTiming = table.Column<string>(type: "text", nullable: true),
                    Direction = table.Column<byte>(type: "smallint", nullable: false),
                    AltitudeDescription = table.Column<byte>(type: "smallint", nullable: false),
                    IsAltitudeModifiable = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude2_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude2_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    TransitionAltitude = table.Column<int>(type: "integer", nullable: false),
                    SpeedLimit = table.Column<int>(type: "integer", nullable: false),
                    MultiplierOrTurn = table.Column<char>(type: "character(1)", nullable: false),
                    Overlay = table.Column<byte>(type: "smallint", nullable: false),
                    SpeedLimitType = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApproachSequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Types = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Transition = table.Column<string>(type: "text", nullable: true),
                    AircraftTypes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachSequence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrivalAltitude",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FixPosition = table.Column<byte>(type: "smallint", nullable: false),
                    CourseType = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalAltitude", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrivalPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VerticalAngle = table.Column<float>(type: "real", nullable: false),
                    Qualifiers = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    Descriptions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Turn = table.Column<byte>(type: "smallint", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    LegType = table.Column<byte>(type: "smallint", nullable: false),
                    IsTurnRequired = table.Column<byte>(type: "smallint", nullable: false),
                    ArcRadius = table.Column<float>(type: "real", nullable: false),
                    Theta = table.Column<float>(type: "real", nullable: false),
                    Rho = table.Column<float>(type: "real", nullable: false),
                    Course_Value = table.Column<float>(type: "real", nullable: false),
                    Course_Type = table.Column<byte>(type: "smallint", nullable: false),
                    DistanceTiming = table.Column<string>(type: "text", nullable: true),
                    Direction = table.Column<byte>(type: "smallint", nullable: false),
                    AltitudeDescription = table.Column<byte>(type: "smallint", nullable: false),
                    IsAltitudeModifiable = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude2_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude2_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    TransitionAltitude = table.Column<int>(type: "integer", nullable: false),
                    SpeedLimit = table.Column<int>(type: "integer", nullable: false),
                    MultiplierOrTurn = table.Column<char>(type: "character(1)", nullable: false),
                    Overlay = table.Column<byte>(type: "smallint", nullable: false),
                    SpeedLimitType = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Arrivals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrivals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrivalSequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Types = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Transition = table.Column<string>(type: "text", nullable: true),
                    AircraftTypes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalSequence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlledAirspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlledAirspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControlledVolume",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Multiplier = table.Column<char>(type: "character(1)", nullable: true),
                    LevelType = table.Column<byte>(type: "smallint", nullable: false),
                    TimeCode = table.Column<byte>(type: "smallint", nullable: false),
                    Notam = table.Column<char>(type: "character(1)", nullable: false),
                    Low_Value = table.Column<float>(type: "real", nullable: false),
                    Low_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    LowUnit = table.Column<byte>(type: "smallint", nullable: false),
                    Up_Value = table.Column<float>(type: "real", nullable: false),
                    Up_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    UpUnit = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlledVolume", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CruiseColumn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    From = table.Column<float>(type: "real", nullable: false),
                    To = table.Column<float>(type: "real", nullable: false),
                    CourseType = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CruiseColumn", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CruiseTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CruiseTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeparturePoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Qualifiers = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    Descriptions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Turn = table.Column<byte>(type: "smallint", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    LegType = table.Column<byte>(type: "smallint", nullable: false),
                    IsTurnRequired = table.Column<byte>(type: "smallint", nullable: false),
                    ArcRadius = table.Column<float>(type: "real", nullable: false),
                    Theta = table.Column<float>(type: "real", nullable: false),
                    Rho = table.Column<float>(type: "real", nullable: false),
                    Course_Value = table.Column<float>(type: "real", nullable: false),
                    Course_Type = table.Column<byte>(type: "smallint", nullable: false),
                    DistanceTiming = table.Column<string>(type: "text", nullable: true),
                    Direction = table.Column<byte>(type: "smallint", nullable: false),
                    AltitudeDescription = table.Column<byte>(type: "smallint", nullable: false),
                    IsAltitudeModifiable = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude2_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude2_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    TransitionAltitude = table.Column<int>(type: "integer", nullable: false),
                    SpeedLimit = table.Column<int>(type: "integer", nullable: false),
                    MultiplierOrTurn = table.Column<char>(type: "character(1)", nullable: false),
                    Overlay = table.Column<byte>(type: "smallint", nullable: false),
                    SpeedLimitType = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeparturePoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartureSequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Types = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Transition = table.Column<string>(type: "text", nullable: true),
                    AircraftTypes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartureSequence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AirportIdentifier = table.Column<string>(type: "text", nullable: false),
                    IcaoCode = table.Column<string>(type: "text", nullable: false),
                    ProcedureIdentifier = table.Column<string>(type: "text", nullable: false),
                    ProcedureType = table.Column<char>(type: "character(1)", nullable: false),
                    RunwayTransitionIdentifier = table.Column<string>(type: "text", nullable: false),
                    RunwayTransitionFix = table.Column<string>(type: "text", nullable: false),
                    RunwayTransitionFixIcaoCode = table.Column<string>(type: "text", nullable: false),
                    RunwayTransitionFixSectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    FixSubsectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    RunwayTransitionAlongTrackDistance = table.Column<string>(type: "text", nullable: false),
                    CommonSegmentTransitionFix = table.Column<string>(type: "text", nullable: false),
                    CommonSegmentTransitionFixIcaoCode = table.Column<string>(type: "text", nullable: false),
                    CommonSegmentTransitionFixSectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    CommonSegmentTransitionFixSubsectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    CommonSegmentAlongTrackDistance = table.Column<string>(type: "text", nullable: false),
                    EnrouteTransitionIdentifier = table.Column<string>(type: "text", nullable: false),
                    EnrouteTransitionFix = table.Column<string>(type: "text", nullable: false),
                    EnrouteTransitionFixIcaoCode = table.Column<string>(type: "text", nullable: false),
                    EnrouteTransitionFixSectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    EnrouteTransitionFixSubsectionCode = table.Column<char>(type: "character(1)", nullable: false),
                    EnrouteTransitionAlongTrackDistance = table.Column<string>(type: "text", nullable: false),
                    SequenceNumber = table.Column<string>(type: "text", nullable: false),
                    ContinuationRecordNumber = table.Column<char>(type: "character(1)", nullable: false),
                    EnginesNumber = table.Column<string>(type: "text", nullable: false),
                    EngineTypeRestriction = table.Column<char>(type: "character(1)", nullable: false),
                    IsRnav = table.Column<char>(type: "character(1)", nullable: false),
                    AtcWeightCategory = table.Column<char>(type: "character(1)", nullable: false),
                    AtcIdentifier = table.Column<string>(type: "text", nullable: false),
                    TimeCode = table.Column<char>(type: "character(1)", nullable: false),
                    ProcedureDescription = table.Column<string>(type: "text", nullable: false),
                    LegTypeCode = table.Column<string>(type: "text", nullable: false),
                    ReportingCode = table.Column<char>(type: "character(1)", nullable: false),
                    InitialDepartureMagneticCourse = table.Column<string>(type: "text", nullable: false),
                    AltitudeDescription = table.Column<char>(type: "character(1)", nullable: false),
                    FirstAltitude = table.Column<string>(type: "text", nullable: false),
                    SecondAltitude = table.Column<string>(type: "text", nullable: false),
                    SpeedLimit = table.Column<string>(type: "text", nullable: false),
                    InitialCruiseTable = table.Column<string>(type: "text", nullable: false),
                    SpeedLimitDescription = table.Column<char>(type: "character(1)", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeographicalReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicalReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalLandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Radius = table.Column<int>(type: "integer", nullable: false),
                    Slots = table.Column<byte>(type: "smallint", nullable: false),
                    SlopeAngle = table.Column<float>(type: "real", nullable: false),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    StationType = table.Column<string>(type: "text", nullable: true),
                    ElevationWgs84 = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Bearing_Value = table.Column<float>(type: "real", nullable: false),
                    Bearing_Type = table.Column<byte>(type: "smallint", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalLandings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroundPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    AsRunway = table.Column<string>(type: "text", nullable: false),
                    RouteIndicator = table.Column<char>(type: "character(1)", nullable: false),
                    PathSelector = table.Column<int>(type: "integer", nullable: false),
                    ApproachPerformance = table.Column<byte>(type: "smallint", nullable: false),
                    EllipsoidalHeight = table.Column<float>(type: "real", nullable: false),
                    GlideAngle = table.Column<float>(type: "real", nullable: false),
                    AlignmentCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    AlignmentCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    CourseWidth = table.Column<float>(type: "real", nullable: false),
                    LengthOffset = table.Column<int>(type: "integer", nullable: false),
                    ThresholdHeight_Value = table.Column<float>(type: "real", nullable: false),
                    ThresholdHeight_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Remainder = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroundPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HelicopterCompanyRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelicopterCompanyRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoldingPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    DuplicateIndicator = table.Column<string>(type: "text", nullable: true),
                    In_Value = table.Column<float>(type: "real", nullable: false),
                    In_Type = table.Column<byte>(type: "smallint", nullable: false),
                    Turn = table.Column<byte>(type: "smallint", nullable: false),
                    LegLength = table.Column<float>(type: "real", nullable: false),
                    LegTime = table.Column<float>(type: "real", nullable: false),
                    Minimum_Value = table.Column<float>(type: "real", nullable: false),
                    Minimum_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Maximum_Value = table.Column<float>(type: "real", nullable: false),
                    Maximum_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    ArcRadius = table.Column<float>(type: "real", nullable: false),
                    ScaleFactor = table.Column<int>(type: "integer", nullable: false),
                    MinLevel = table.Column<int>(type: "integer", nullable: false),
                    MaxLevel = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<byte>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingPatterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentLandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    GlideSlopeCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    GlideSlopeCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    PositionReference = table.Column<char>(type: "character(1)", nullable: false),
                    GlideSlopePosition = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<float>(type: "real", nullable: false),
                    SlopeAngle = table.Column<float>(type: "real", nullable: false),
                    Declination = table.Column<string>(type: "text", nullable: false),
                    ThresholdHeight = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Bearing_Value = table.Column<float>(type: "real", nullable: false),
                    Bearing_Type = table.Column<byte>(type: "smallint", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentLandings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentMarker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Frequency = table.Column<float>(type: "real", nullable: false),
                    Bearing = table.Column<float>(type: "real", nullable: false),
                    LocatorCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    LocatorCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    NavaidType = table.Column<byte>(type: "smallint", nullable: false),
                    Coverage = table.Column<byte>(type: "smallint", nullable: false),
                    Info = table.Column<byte>(type: "smallint", nullable: false),
                    Collocation = table.Column<byte>(type: "smallint", nullable: false),
                    Facility = table.Column<string>(type: "text", nullable: true),
                    LocatorIdentifier = table.Column<string>(type: "text", nullable: true),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentMarker", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MicrowaveLandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    ElevationCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    ElevationCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    PositionReference = table.Column<char>(type: "character(1)", nullable: false),
                    ElevationPosition = table.Column<int>(type: "integer", nullable: false),
                    RightAngle = table.Column<int>(type: "integer", nullable: false),
                    LeftAngle = table.Column<int>(type: "integer", nullable: false),
                    RightCoverage = table.Column<int>(type: "integer", nullable: false),
                    LeftCoverage = table.Column<int>(type: "integer", nullable: false),
                    AngleSpan = table.Column<float>(type: "real", nullable: false),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    NominalElevationAngle = table.Column<float>(type: "real", nullable: false),
                    MinimumGlideAngle = table.Column<float>(type: "real", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Bearing_Value = table.Column<float>(type: "real", nullable: false),
                    Bearing_Type = table.Column<byte>(type: "smallint", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrowaveLandings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinimumAltitude",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Multiplier = table.Column<char>(type: "character(1)", nullable: true),
                    CourseType = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinimumAltitude", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nondirects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Coverage = table.Column<byte>(type: "smallint", nullable: false),
                    Info = table.Column<byte>(type: "smallint", nullable: false),
                    Collocation = table.Column<byte>(type: "smallint", nullable: false),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nondirects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Omnidirects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Coverage = table.Column<byte>(type: "smallint", nullable: false),
                    Info = table.Column<byte>(type: "smallint", nullable: false),
                    Collocation = table.Column<byte>(type: "smallint", nullable: false),
                    EquipmentIdentifier = table.Column<string>(type: "text", nullable: true),
                    EquipmentCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: true),
                    EquipmentCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: true),
                    EquipmentElevation = table.Column<int>(type: "integer", nullable: false),
                    Range = table.Column<byte>(type: "smallint", nullable: false),
                    EquipmentOffset = table.Column<float>(type: "real", nullable: false),
                    ProtectionDistance = table.Column<int>(type: "integer", nullable: false),
                    NotAreaNavigation = table.Column<byte>(type: "smallint", nullable: false),
                    ServiceVolume = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Omnidirects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Port",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Designator = table.Column<string>(type: "text", nullable: true),
                    Limit_Value = table.Column<float>(type: "real", nullable: false),
                    Limit_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    IsProcedurePublished = table.Column<byte>(type: "smallint", nullable: false),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false),
                    SpeedLimit = table.Column<int>(type: "integer", nullable: false),
                    TransitionAltitude = table.Column<int>(type: "integer", nullable: true),
                    TransitionLevel = table.Column<int>(type: "integer", nullable: true),
                    Privacy = table.Column<byte>(type: "smallint", nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    IsDaylightTime = table.Column<byte>(type: "smallint", nullable: false),
                    CourseType = table.Column<byte>(type: "smallint", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    LongestRunwayLength = table.Column<int>(type: "integer", nullable: true),
                    LongestRunwayType = table.Column<byte>(type: "smallint", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Port", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortTransmitter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsMultiSector = table.Column<byte>(type: "smallint", nullable: false),
                    Limitation = table.Column<byte>(type: "smallint", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    Usages = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    SeqNumber = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Frequency_Value = table.Column<double>(type: "double precision", nullable: false),
                    Frequency_Unit = table.Column<string>(type: "text", nullable: false),
                    IsRadarAvailable = table.Column<byte>(type: "smallint", nullable: false),
                    IsWholeDay = table.Column<byte>(type: "smallint", nullable: false),
                    CallSign = table.Column<string>(type: "text", nullable: true),
                    AltitudeDescription = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Altitude2_Value = table.Column<float>(type: "real", nullable: false),
                    Altitude2_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Modulation = table.Column<byte>(type: "smallint", nullable: false),
                    Emission = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortTransmitter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreferredRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegionVolume",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    SpeedReportUnit = table.Column<byte>(type: "smallint", nullable: false),
                    AltitudeReportUnit = table.Column<byte>(type: "smallint", nullable: false),
                    IsEntryReport = table.Column<byte>(type: "smallint", nullable: false),
                    Up_Value = table.Column<float>(type: "real", nullable: false),
                    Up_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    UpperRegionLow_Value = table.Column<float>(type: "real", nullable: false),
                    UpperRegionLow_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    UpperRegionUp_Value = table.Column<float>(type: "real", nullable: false),
                    UpperRegionUp_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionVolume", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestrictiveAirspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictiveAirspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestrictiveVolume",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Multiplier = table.Column<char>(type: "character(1)", nullable: true),
                    LevelType = table.Column<byte>(type: "smallint", nullable: false),
                    TimeCode = table.Column<byte>(type: "smallint", nullable: false),
                    Notam = table.Column<char>(type: "character(1)", nullable: false),
                    Low_Value = table.Column<float>(type: "real", nullable: false),
                    Low_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    LowUnit = table.Column<byte>(type: "smallint", nullable: false),
                    Up_Value = table.Column<float>(type: "real", nullable: false),
                    Up_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    UpUnit = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictiveVolume", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SatellitePoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Service = table.Column<string>(type: "text", nullable: false),
                    HorizontalAlert = table.Column<float>(type: "real", nullable: false),
                    VerticalAlert = table.Column<float>(type: "real", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    AsRunway = table.Column<string>(type: "text", nullable: false),
                    RouteIndicator = table.Column<char>(type: "character(1)", nullable: false),
                    PathSelector = table.Column<int>(type: "integer", nullable: false),
                    ApproachPerformance = table.Column<byte>(type: "smallint", nullable: false),
                    EllipsoidalHeight = table.Column<float>(type: "real", nullable: false),
                    GlideAngle = table.Column<float>(type: "real", nullable: false),
                    AlignmentCoordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    AlignmentCoordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    CourseWidth = table.Column<float>(type: "real", nullable: false),
                    LengthOffset = table.Column<int>(type: "integer", nullable: false),
                    ThresholdHeight_Value = table.Column<float>(type: "real", nullable: false),
                    ThresholdHeight_Unit = table.Column<byte>(type: "smallint", nullable: false),
                    Remainder = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatellitePoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tacticals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Coverage = table.Column<byte>(type: "smallint", nullable: false),
                    Info = table.Column<byte>(type: "smallint", nullable: false),
                    Collocation = table.Column<byte>(type: "smallint", nullable: false),
                    TacanIdentifier = table.Column<string>(type: "text", nullable: false),
                    Elevation = table.Column<int>(type: "integer", nullable: false),
                    Range = table.Column<byte>(type: "smallint", nullable: false),
                    ProtectionDistance = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tacticals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Touch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    Bearing_Value = table.Column<float>(type: "real", nullable: true),
                    Bearing_Type = table.Column<byte>(type: "smallint", nullable: true),
                    Gradient = table.Column<float>(type: "real", nullable: true),
                    EllipsoidalHeight = table.Column<float>(type: "real", nullable: true),
                    Elevation = table.Column<int>(type: "integer", nullable: true),
                    Distance = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<byte>(type: "smallint", nullable: true),
                    Stopway = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Touch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    OAuthProvider = table.Column<string>(type: "text", nullable: false),
                    OAuthSubjectId = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsGuest = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Waypoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Types = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Usages = table.Column<byte>(type: "smallint", nullable: false),
                    Variation = table.Column<float>(type: "real", nullable: false),
                    Datum = table.Column<string>(type: "text", nullable: true),
                    NameFormats = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<int>(type: "integer", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Icao_First = table.Column<char>(type: "character(1)", nullable: false),
                    Icao_Second = table.Column<char>(type: "character(1)", nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waypoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AirportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindDirectionDeg = table.Column<int>(type: "integer", nullable: true),
                    WindSpeedKts = table.Column<int>(type: "integer", nullable: true),
                    WindGustKts = table.Column<int>(type: "integer", nullable: true),
                    VisibilitySm = table.Column<decimal>(type: "numeric", nullable: true),
                    CeilingFt = table.Column<int>(type: "integer", nullable: true),
                    TemperatureC = table.Column<int>(type: "integer", nullable: true),
                    DewpointC = table.Column<int>(type: "integer", nullable: true),
                    AltimeterInHg = table.Column<decimal>(type: "numeric", nullable: true),
                    WeatherPhenomena = table.Column<string[]>(type: "text[]", nullable: true),
                    MetarRaw = table.Column<string>(type: "text", nullable: true),
                    TafRaw = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DifficultyLevel = table.Column<int>(type: "integer", nullable: false),
                    AirportCode = table.Column<string>(type: "text", nullable: false),
                    ScenarioType = table.Column<string>(type: "text", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxAircraft = table.Column<int>(type: "integer", nullable: true),
                    WeatherConditions = table.Column<string>(type: "text", nullable: true),
                    InitialAircraftStates = table.Column<string>(type: "text", nullable: true),
                    ActiveRunways = table.Column<string[]>(type: "text[]", nullable: true),
                    ActiveFrequencies = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    PlayCount = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<float>(type: "real", nullable: true),
                    Tags = table.Column<string[]>(type: "text[]", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenarios_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Progress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => new { x.UserId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedScenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaveName = table.Column<string>(type: "text", nullable: true),
                    SimulationState = table.Column<byte[]>(type: "bytea", nullable: false),
                    SimulationTime = table.Column<float>(type: "real", nullable: false),
                    CurrentScore = table.Column<int>(type: "integer", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedScenarios_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedScenarios_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    AircraftControlled = table.Column<int>(type: "integer", nullable: false),
                    CommandsIssued = table.Column<int>(type: "integer", nullable: false),
                    SeparationViolations = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulLandings = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulHandoffs = table.Column<int>(type: "integer", nullable: false),
                    TimeAcceleration = table.Column<float>(type: "real", nullable: false),
                    FinalScoreBreakdown = table.Column<string>(type: "text", nullable: true),
                    StateSnapshot = table.Column<byte[]>(type: "bytea", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScoreValue = table.Column<int>(type: "integer", nullable: false),
                    TimeAcceleration = table.Column<float>(type: "real", nullable: false),
                    AdjustedScore = table.Column<int>(type: "integer", nullable: false),
                    AircraftControlled = table.Column<int>(type: "integer", nullable: false),
                    CommandsIssued = table.Column<int>(type: "integer", nullable: false),
                    EfficiencyRating = table.Column<float>(type: "real", nullable: true),
                    SafetyRating = table.Column<float>(type: "real", nullable: true),
                    AchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scores_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "Scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scores_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionCommands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimulationTime = table.Column<float>(type: "real", nullable: false),
                    AircraftId = table.Column<string>(type: "text", nullable: false),
                    CommandType = table.Column<string>(type: "text", nullable: false),
                    CommandText = table.Column<string>(type: "text", nullable: false),
                    CommandParams = table.Column<string>(type: "text", nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionCommands_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SimulationTime = table.Column<float>(type: "real", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    AircraftIds = table.Column<string[]>(type: "text[]", nullable: true),
                    EventData = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionEvents_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedScenarios_ScenarioId",
                table: "SavedScenarios",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedScenarios_UserId",
                table: "SavedScenarios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_CreatorId",
                table: "Scenarios",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_ScenarioId",
                table: "Scores",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_SessionId",
                table: "Scores",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_UserId",
                table: "Scores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionCommands_SessionId",
                table: "SessionCommands",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvents_SessionId",
                table: "SessionEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScenarioId",
                table: "Sessions",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_types");

            migrationBuilder.DropTable(
                name: "AirportCommunications");

            migrationBuilder.DropTable(
                name: "AirwayCommunications");

            migrationBuilder.DropTable(
                name: "AirwayPoint");

            migrationBuilder.DropTable(
                name: "Airways");

            migrationBuilder.DropTable(
                name: "Approaches");

            migrationBuilder.DropTable(
                name: "ApproachPoint");

            migrationBuilder.DropTable(
                name: "ApproachSequence");

            migrationBuilder.DropTable(
                name: "ArrivalAltitude");

            migrationBuilder.DropTable(
                name: "ArrivalPoint");

            migrationBuilder.DropTable(
                name: "Arrivals");

            migrationBuilder.DropTable(
                name: "ArrivalSequence");

            migrationBuilder.DropTable(
                name: "CommunicationTypes");

            migrationBuilder.DropTable(
                name: "CompanyRoutes");

            migrationBuilder.DropTable(
                name: "ControlledAirspaces");

            migrationBuilder.DropTable(
                name: "ControlledVolume");

            migrationBuilder.DropTable(
                name: "CruiseColumn");

            migrationBuilder.DropTable(
                name: "CruiseTables");

            migrationBuilder.DropTable(
                name: "DeparturePoint");

            migrationBuilder.DropTable(
                name: "Departures");

            migrationBuilder.DropTable(
                name: "DepartureSequence");

            migrationBuilder.DropTable(
                name: "FlightPlans");

            migrationBuilder.DropTable(
                name: "FlightRegions");

            migrationBuilder.DropTable(
                name: "Gates");

            migrationBuilder.DropTable(
                name: "GeographicalReferences");

            migrationBuilder.DropTable(
                name: "GlobalLandings");

            migrationBuilder.DropTable(
                name: "GroundPoint");

            migrationBuilder.DropTable(
                name: "HelicopterCompanyRoutes");

            migrationBuilder.DropTable(
                name: "HoldingPatterns");

            migrationBuilder.DropTable(
                name: "InstrumentLandings");

            migrationBuilder.DropTable(
                name: "InstrumentMarker");

            migrationBuilder.DropTable(
                name: "MicrowaveLandings");

            migrationBuilder.DropTable(
                name: "MinimumAltitude");

            migrationBuilder.DropTable(
                name: "Nondirects");

            migrationBuilder.DropTable(
                name: "Omnidirects");

            migrationBuilder.DropTable(
                name: "Port");

            migrationBuilder.DropTable(
                name: "PortTransmitter");

            migrationBuilder.DropTable(
                name: "PreferredRoutes");

            migrationBuilder.DropTable(
                name: "RegionVolume");

            migrationBuilder.DropTable(
                name: "RestrictiveAirspaces");

            migrationBuilder.DropTable(
                name: "RestrictiveVolume");

            migrationBuilder.DropTable(
                name: "SatellitePoint");

            migrationBuilder.DropTable(
                name: "SavedScenarios");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "SessionCommands");

            migrationBuilder.DropTable(
                name: "SessionEvents");

            migrationBuilder.DropTable(
                name: "Tacticals");

            migrationBuilder.DropTable(
                name: "Touch");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "Waypoints");

            migrationBuilder.DropTable(
                name: "WeatherRecords");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Scenarios");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
