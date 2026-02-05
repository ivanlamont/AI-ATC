using System;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Sample navigation database with realistic fixes and procedures for KSFO (San Francisco)
/// </summary>
public static class SampleNavigationData
{
    /// <summary>
    /// Creates a navigation database populated with sample data for KSFO area
    /// </summary>
    public static NavigationDatabase CreateSampleDatabase()
    {
        var db = new NavigationDatabase();

        // Add fixes around KSFO
        // These are realistic waypoint names from the San Francisco area
        // Positions are approximate in local NM coordinates (origin at KSFO)

        // Final approach fixes
        db.AddFix(new Fix
        {
            Identifier = "CEPIN",
            PositionNm = new Vector2(-5, 0),   // 5nm west (on ILS 28 centerline)
            Type = FixType.GPS,
            Description = "Final approach fix for ILS 28L"
        });

        db.AddFix(new Fix
        {
            Identifier = "FAITH",
            PositionNm = new Vector2(5, 0),    // 5nm east (on ILS 10 centerline)
            Type = FixType.GPS,
            Description = "Final approach fix for ILS 10L"
        });

        // Arrival fixes
        db.AddFix(new Fix
        {
            Identifier = "EDDYY",
            PositionNm = new Vector2(-30, -15),
            Type = FixType.GPS,
            Description = "Arrival fix from the south"
        });

        db.AddFix(new Fix
        {
            Identifier = "ARCHI",
            PositionNm = new Vector2(-20, 25),
            Type = FixType.GPS,
            Description = "Arrival fix from the north"
        });

        db.AddFix(new Fix
        {
            Identifier = "MOVDD",
            PositionNm = new Vector2(20, -20),
            Type = FixType.GPS,
            Description = "Arrival fix from the east"
        });

        // Intermediate fixes
        db.AddFix(new Fix
        {
            Identifier = "DUMBA",
            PositionNm = new Vector2(-15, 10),
            Type = FixType.GPS,
            Description = "Downwind fix for runway 28"
        });

        db.AddFix(new Fix
        {
            Identifier = "BGGLO",
            PositionNm = new Vector2(-10, -8),
            Type = FixType.GPS,
            Description = "Base turn fix"
        });

        db.AddFix(new Fix
        {
            Identifier = "SUNST",
            PositionNm = new Vector2(10, 10),
            Type = FixType.GPS,
            Description = "Holding fix"
        });

        db.AddFix(new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(-8, 15),
            Type = FixType.GPS,
            Description = "Downwind entry fix"
        });

        // VOR stations
        db.AddFix(new Fix
        {
            Identifier = "KSFO",
            PositionNm = new Vector2(0, 0),
            Type = FixType.VOR,
            Description = "San Francisco VOR"
        });

        // Add STAR procedures
        var star1 = new Procedure
        {
            Identifier = "BDEGA2",
            Type = ProcedureType.STAR,
            AirportIdentifier = "KSFO",
            Description = "BDEGA TWO arrival",
            InitialFix = db.GetFix("EDDYY"),
            FinalFix = db.GetFix("DUMBA")
        };
        star1.Route.AddFix(db.GetFix("EDDYY")!);
        star1.Route.AddFix(db.GetFix("BGGLO")!);
        star1.Route.AddFix(db.GetFix("DUMBA")!);
        star1.Route.Segments[1].AltitudeConstraintFt = 10000;
        star1.Route.Segments[1].ConstraintType = AltitudeConstraintType.AtOrBelow;
        star1.Route.Segments[2].AltitudeConstraintFt = 5000;
        star1.Route.Segments[2].ConstraintType = AltitudeConstraintType.At;
        db.AddProcedure(star1);

        var star2 = new Procedure
        {
            Identifier = "DYAMD3",
            Type = ProcedureType.STAR,
            AirportIdentifier = "KSFO",
            Description = "DYAMD THREE arrival",
            InitialFix = db.GetFix("ARCHI"),
            FinalFix = db.GetFix("BEBOP")
        };
        star2.Route.AddFix(db.GetFix("ARCHI")!);
        star2.Route.AddFix(db.GetFix("BEBOP")!);
        star2.Route.AddFix(db.GetFix("DUMBA")!);
        star2.Route.Segments[1].AltitudeConstraintFt = 8000;
        star2.Route.Segments[1].ConstraintType = AltitudeConstraintType.AtOrAbove;
        star2.Route.Segments[2].AltitudeConstraintFt = 5000;
        star2.Route.Segments[2].ConstraintType = AltitudeConstraintType.At;
        db.AddProcedure(star2);

        // Add SID procedures
        var sid1 = new Procedure
        {
            Identifier = "BGGLO2",
            Type = ProcedureType.SID,
            AirportIdentifier = "KSFO",
            RunwayIdentifier = "28L",
            Description = "BGGLO TWO departure",
            InitialFix = db.GetFix("KSFO"),
            FinalFix = db.GetFix("BGGLO")
        };
        sid1.Route.AddFix(db.GetFix("KSFO")!);
        sid1.Route.AddFix(db.GetFix("BGGLO")!);
        sid1.Route.Segments[1].AltitudeConstraintFt = 3000;
        sid1.Route.Segments[1].ConstraintType = AltitudeConstraintType.AtOrAbove;
        db.AddProcedure(sid1);

        // Add approach procedures
        var ils28 = new Procedure
        {
            Identifier = "ILS28L",
            Type = ProcedureType.Approach,
            AirportIdentifier = "KSFO",
            RunwayIdentifier = "28L",
            Description = "ILS or LOC RWY 28L",
            InitialFix = db.GetFix("DUMBA"),
            FinalFix = db.GetFix("CEPIN")
        };
        ils28.Route.AddFix(db.GetFix("DUMBA")!);
        ils28.Route.AddFix(db.GetFix("CEPIN")!);
        ils28.Route.Segments[1].AltitudeConstraintFt = 2100;
        ils28.Route.Segments[1].ConstraintType = AltitudeConstraintType.At;
        ils28.Route.Segments[1].SpeedConstraintKnots = 180;
        db.AddProcedure(ils28);

        var rnav28 = new Procedure
        {
            Identifier = "RNAV28L",
            Type = ProcedureType.Approach,
            AirportIdentifier = "KSFO",
            RunwayIdentifier = "28L",
            Description = "RNAV (GPS) RWY 28L",
            InitialFix = db.GetFix("DUMBA"),
            FinalFix = db.GetFix("CEPIN")
        };
        rnav28.Route.AddFix(db.GetFix("DUMBA")!);
        rnav28.Route.AddFix(db.GetFix("CEPIN")!);
        rnav28.Route.Segments[1].AltitudeConstraintFt = 2100;
        rnav28.Route.Segments[1].ConstraintType = AltitudeConstraintType.At;
        db.AddProcedure(rnav28);

        return db;
    }

    /// <summary>
    /// Creates a simple test database with basic fixes
    /// </summary>
    public static NavigationDatabase CreateTestDatabase()
    {
        var db = new NavigationDatabase();

        // Simple 4-corner pattern
        db.AddFix(new Fix
        {
            Identifier = "NORTH",
            PositionNm = new Vector2(0, 10),
            Type = FixType.GPS
        });

        db.AddFix(new Fix
        {
            Identifier = "SOUTH",
            PositionNm = new Vector2(0, -10),
            Type = FixType.GPS
        });

        db.AddFix(new Fix
        {
            Identifier = "EAST",
            PositionNm = new Vector2(10, 0),
            Type = FixType.GPS
        });

        db.AddFix(new Fix
        {
            Identifier = "WEST",
            PositionNm = new Vector2(-10, 0),
            Type = FixType.GPS
        });

        return db;
    }
}
