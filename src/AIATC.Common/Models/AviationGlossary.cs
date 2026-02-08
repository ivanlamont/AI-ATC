using System.Text.Json.Serialization;

namespace AIATC.Common
{
    /// <summary>
    /// Aviation Glossary data model for ATC phraseology and terminology
    /// </summary>
    public class AviationGlossary
    {
        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("language_style")]
        public LanguageStyle? LanguageStyle { get; set; }

        [JsonPropertyName("entities")]
        public Entities? Entities { get; set; }

        [JsonPropertyName("actions")]
        public Actions? Actions { get; set; }

        [JsonPropertyName("clearances")]
        public Clearances? Clearances { get; set; }

        [JsonPropertyName("navigation")]
        public Navigation? Navigation { get; set; }

        [JsonPropertyName("airspace")]
        public Airspace? Airspace { get; set; }

        [JsonPropertyName("surveillance")]
        public Surveillance? Surveillance { get; set; }

        [JsonPropertyName("airport_surface")]
        public AirportSurface? AirportSurface { get; set; }

        [JsonPropertyName("weather")]
        public Weather? Weather { get; set; }

        [JsonPropertyName("phraseology_rules")]
        public PhraseologyRules? PhraseologyRules { get; set; }
    }

    public class LanguageStyle
    {
        [JsonPropertyName("tone")]
        public string[]? Tone { get; set; }

        [JsonPropertyName("constraints")]
        public Constraints? Constraints { get; set; }
    }

    public class Constraints
    {
        [JsonPropertyName("avoid_explanations")]
        public bool AvoidExplanations { get; set; }

        [JsonPropertyName("prefer_imperatives")]
        public bool PreferImperatives { get; set; }

        [JsonPropertyName("assume_shared_context")]
        public bool AssumeSharedContext { get; set; }

        [JsonPropertyName("safety_priority")]
        public string? SafetyPriority { get; set; }
    }

    public class Entities
    {
        [JsonPropertyName("roles")]
        public Roles? Roles { get; set; }

        [JsonPropertyName("aircraft")]
        public Aircraft? Aircraft { get; set; }

        [JsonPropertyName("facility")]
        public Facility? Facility { get; set; }
    }

    public class Roles
    {
        [JsonPropertyName("ATC")]
        public RoleDefinition? ATC { get; set; }

        [JsonPropertyName("Pilot")]
        public RoleDefinition? Pilot { get; set; }

        [JsonPropertyName("Facility")]
        public Facility? Facility { get; set; }
    }

    public class RoleDefinition
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class Aircraft
    {
        [JsonPropertyName("classification")]
        public Classification? Classification { get; set; }

        [JsonPropertyName("state")]
        public string[]? State { get; set; }
    }

    public class Classification
    {
        [JsonPropertyName("approach_category")]
        public string[]? ApproachCategory { get; set; }

        [JsonPropertyName("wake_turbulence")]
        public string[]? WakeTurbulence { get; set; }
    }

    public class Facility
    {
        [JsonPropertyName("types")]
        public string[]? Types { get; set; }
    }

    public class Actions
    {
        [JsonPropertyName("controller_verbs")]
        public string[]? ControllerVerbs { get; set; }

        [JsonPropertyName("pilot_responses")]
        public string[]? PilotResponses { get; set; }
    }

    public class Clearances
    {
        [JsonPropertyName("types")]
        public string[]? Types { get; set; }

        [JsonPropertyName("constraints")]
        public string[]? Constraints { get; set; }
    }

    public class Navigation
    {
        [JsonPropertyName("procedures")]
        public string[]? Procedures { get; set; }

        [JsonPropertyName("fixes")]
        public string[]? Fixes { get; set; }

        [JsonPropertyName("geometry")]
        public string[]? Geometry { get; set; }
    }

    public class Airspace
    {
        [JsonPropertyName("classes")]
        public string[]? Classes { get; set; }

        [JsonPropertyName("special_use")]
        public string[]? SpecialUse { get; set; }

        [JsonPropertyName("flow_management")]
        public string[]? FlowManagement { get; set; }
    }

    public class Surveillance
    {
        [JsonPropertyName("systems")]
        public string[]? Systems { get; set; }

        [JsonPropertyName("automation")]
        public string[]? Automation { get; set; }

        [JsonPropertyName("conflict_states")]
        public ConflictState[]? ConflictStates { get; set; }
    }

    public class ConflictState
    {
        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("separation_nm_max")]
        public double? SeparationNmMax { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class AirportSurface
    {
        [JsonPropertyName("areas")]
        public string[]? Areas { get; set; }

        [JsonPropertyName("operations")]
        public string[]? Operations { get; set; }

        [JsonPropertyName("runway_conditions")]
        public RunwayConditions? RunwayConditions { get; set; }
    }

    public class RunwayConditions
    {
        [JsonPropertyName("braking_action")]
        public string[]? BrakingAction { get; set; }

        [JsonPropertyName("codes")]
        public string? Codes { get; set; }
    }

    public class Weather
    {
        [JsonPropertyName("products")]
        public string[]? Products { get; set; }

        [JsonPropertyName("phenomena")]
        public string[]? Phenomena { get; set; }
    }

    public class PhraseologyRules
    {
        [JsonPropertyName("acknowledgement_required")]
        public string[]? AcknowledgementRequired { get; set; }

        [JsonPropertyName("broadcast_only")]
        public string[]? BroadcastOnly { get; set; }

        [JsonPropertyName("non_control_prefixes")]
        public string[]? NonControlPrefixes { get; set; }
    }
}