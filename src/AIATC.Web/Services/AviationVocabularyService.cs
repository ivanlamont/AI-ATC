using System.Text.Json;
using AIATC.Common;

namespace AIATC.Web.Services
{
    /// <summary>
    /// Aviation Vocabulary Service for ATC phraseology and specialized terms
    /// Enhances speech recognition and synthesis accuracy for aviation communications
    /// </summary>
    public class AviationVocabularyService
    {
        private AviationGlossary? _glossary;
        private List<string> _flatVocabulary = new();
        private readonly HttpClient _httpClient;
        private readonly ILogger<AviationVocabularyService> _logger;

        public AviationVocabularyService(HttpClient httpClient, ILogger<AviationVocabularyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Load aviation vocabulary from embedded JSON resource
        /// </summary>
        public async Task<bool> LoadVocabularyAsync()
        {
            try
            {
                // Try to load from multiple possible locations
                var possiblePaths = new[]
                {
                    "_content/AIATC.Common/Resources/pilot-controller-glossary.json",
                    "data/pilot-controller-glossary.json",
                    "_framework/pilot-controller-glossary.json"
                };

                foreach (var path in possiblePaths)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(path);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonContent = await response.Content.ReadAsStringAsync();
                            await ProcessVocabularyJson(jsonContent);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to load from {path}: {ex.Message}");
                    }
                }

                _logger.LogError("Could not load aviation vocabulary from any known location");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading aviation vocabulary");
                return false;
            }
        }

        private async Task ProcessVocabularyJson(string jsonContent)
        {
            try
            {
                _glossary = JsonSerializer.Deserialize<AviationGlossary>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_glossary == null)
                {
                    _logger.LogError("Failed to deserialize aviation glossary");
                    return;
                }

                // Extract all vocabulary terms into a flat list for speech services
                _flatVocabulary.Clear();
                ExtractVocabularyTerms();

                _logger.LogInformation($"Loaded aviation vocabulary: {_flatVocabulary.Count} terms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing aviation vocabulary JSON");
            }
        }

        private void ExtractVocabularyTerms()
        {
            if (_glossary == null) return;

            var terms = new HashSet<string>();

            // Add controller verbs
            if (_glossary.Actions?.ControllerVerbs != null)
                terms.UnionWith(_glossary.Actions.ControllerVerbs);

            // Add pilot responses
            if (_glossary.Actions?.PilotResponses != null)
                terms.UnionWith(_glossary.Actions.PilotResponses);

            // Add aircraft classifications
            if (_glossary.Entities?.Aircraft?.Classification?.ApproachCategory != null)
                terms.UnionWith(_glossary.Entities.Aircraft.Classification.ApproachCategory);

            if (_glossary.Entities?.Aircraft?.Classification?.WakeTurbulence != null)
                terms.UnionWith(_glossary.Entities.Aircraft.Classification.WakeTurbulence);

            // Add aircraft states
            if (_glossary.Entities?.Aircraft?.State != null)
                terms.UnionWith(_glossary.Entities.Aircraft.State);

            // Add facility types
            if (_glossary.Entities?.Facility?.Types != null)
                terms.UnionWith(_glossary.Entities.Facility.Types);

            // Add clearance types
            if (_glossary.Clearances?.Types != null)
                terms.UnionWith(_glossary.Clearances.Types);

            // Add navigation procedures
            if (_glossary.Navigation?.Procedures != null)
                terms.UnionWith(_glossary.Navigation.Procedures);

            // Add navigation fixes
            if (_glossary.Navigation?.Fixes != null)
                terms.UnionWith(_glossary.Navigation.Fixes);

            // Add airspace classes
            if (_glossary.Airspace?.Classes != null)
                terms.UnionWith(_glossary.Airspace.Classes);

            // Add surveillance systems
            if (_glossary.Surveillance?.Systems != null)
                terms.UnionWith(_glossary.Surveillance.Systems);

            // Add airport surface areas
            if (_glossary.AirportSurface?.Areas != null)
                terms.UnionWith(_glossary.AirportSurface.Areas);

            // Add airport surface operations
            if (_glossary.AirportSurface?.Operations != null)
                terms.UnionWith(_glossary.AirportSurface.Operations);

            // Add weather products
            if (_glossary.Weather?.Products != null)
                terms.UnionWith(_glossary.Weather.Products);

            // Add weather phenomena
            if (_glossary.Weather?.Phenomena != null)
                terms.UnionWith(_glossary.Weather.Phenomena);

            // Add local scenario radar fix names
            terms.UnionWith(GetLocalScenarioFixNames());

            _flatVocabulary = terms.Where(term => !string.IsNullOrWhiteSpace(term)).ToList();
        }

        /// <summary>
        /// Get local scenario radar fix names from the navigation database
        /// </summary>
        private List<string> GetLocalScenarioFixNames()
        {
            return new List<string>
            {
                // KSFO area fixes from SampleNavigationData
                "CEPIN",    // Final approach fix for ILS 28L
                "FAITH",    // Final approach fix for ILS 10L
                "EDDYY",    // Arrival fix from the south
                "ARCHI",    // Arrival fix from the north
                "MOVDD",    // Arrival fix from the east
                "DUMBA",    // Downwind fix for runway 28
                "BGGLO",    // Base turn fix
                "SUNST",    // Holding fix
                "BEBOP",    // Downwind entry fix
                "KSFO",     // San Francisco VOR
                
                // Simple test fixes
                "NORTH",
                "SOUTH",
                "EAST",
                "WEST"
            };
        }

        /// <summary>
        /// Get flat vocabulary list for speech recognition custom vocabulary
        /// </summary>
        public IReadOnlyList<string> GetVocabularyTerms() => _flatVocabulary.AsReadOnly();

        /// <summary>
        /// Get common ATC phrases for quick selection
        /// </summary>
        public IReadOnlyList<string> GetCommonPhrases()
        {
            return new List<string>
            {
                "CLEARED FOR TAKEOFF",
                "CLEARED TO LAND",
                "TURN LEFT HEADING TWO SEVEN ZERO",
                "CLIMB AND MAINTAIN FLIGHT LEVEL THREE FIVE ZERO",
                "DESCEND AND MAINTAIN TWO THOUSAND",
                "CONTACT DEPARTURE ON ONE TWO ONE POINT NINE",
                "MAINTAIN PRESENT SPEED",
                "REDUCE SPEED TO ONE EIGHT ZERO KNOTS",
                "HOLD SHORT OF RUNWAY",
                "TAXI TO PARKING",
                "UNABLE DUE TRAFFIC",
                "REQUEST HIGHER",
                "WILCO",
                "AFFIRMATIVE",
                "NEGATIVE",
                "STANDBY"
            }.AsReadOnly();
        }

        /// <summary>
        /// Get enhanced pronunciation dictionary for TTS
        /// </summary>
        public Dictionary<string, string> GetPronunciationDictionary()
        {
            return new Dictionary<string, string>
            {
                // Numbers for aviation (spoken individually)
                { "10", "ONE ZERO" },
                { "11", "ONE ONE" },
                { "12", "ONE TWO" },
                { "20", "TWO ZERO" },
                { "30", "THREE ZERO" },

                // Common aviation terms with specific pronunciation
                { "ATIS", "A-TIS" },
                { "ILS", "I-L-S" },
                { "RNAV", "R-NAV" },
                { "GPS", "G-P-S" },
                { "VOR", "V-O-R" },
                { "DME", "D-M-E" },
                { "ADF", "A-D-F" },
                { "TCAS", "T-CAS" },
                { "ACAS", "A-CAS" },

                // Altitudes (with emphasis)
                { "FL350", "FLIGHT LEVEL THREE FIVE ZERO" },
                { "FL100", "FLIGHT LEVEL ONE ZERO ZERO" },
                { "FL200", "FLIGHT LEVEL TWO ZERO ZERO" },

                // Headings (clear pronunciation)
                { "270", "TWO SEVEN ZERO" },
                { "090", "ZERO NINE ZERO" },
                { "180", "ONE EIGHT ZERO" },
                { "360", "THREE SIX ZERO" },

                // Local scenario radar fixes
                { "CEPIN", "C-E-P-I-N" },
                { "FAITH", "FAITH" },
                { "EDDYY", "E-D-D-Y-Y" },
                { "ARCHI", "A-R-C-H-I" },
                { "MOVDD", "M-O-V-D-D" },
                { "DUMBA", "D-U-M-B-A" },
                { "BGGLO", "B-G-G-L-O" },
                { "SUNST", "S-U-N-S-T" },
                { "BEBOP", "B-E-B-O-P" },
                { "KSFO", "K-S-F-O" },
                { "NORTH", "NORTH" },
                { "SOUTH", "SOUTH" },
                { "EAST", "EAST" },
                { "WEST", "WEST" }
            };
        }

        /// <summary>
        /// Check if glossary is loaded
        /// </summary>
        public bool IsLoaded => _glossary != null && _flatVocabulary.Count > 0;

        /// <summary>
        /// Get vocabulary statistics
        /// </summary>
        public VocabularyStats GetStats()
        {
            var localFixNames = GetLocalScenarioFixNames();
            return new VocabularyStats
            {
                TotalTerms = _flatVocabulary.Count,
                ControllerVerbs = _glossary?.Actions?.ControllerVerbs?.Length ?? 0,
                PilotResponses = _glossary?.Actions?.PilotResponses?.Length ?? 0,
                LocalRadarFixes = localFixNames.Count,
                IsLoaded = IsLoaded
            };
        }
    }

    /// <summary>
    /// Vocabulary statistics for monitoring
    /// </summary>
    public class VocabularyStats
    {
        public int TotalTerms { get; set; }
        public int ControllerVerbs { get; set; }
        public int PilotResponses { get; set; }
        public int LocalRadarFixes { get; set; }
        public bool IsLoaded { get; set; }
    }
}