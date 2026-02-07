namespace AIATC.Web.Services;

/// <summary>
/// Configuration service for Azure Cognitive Services integration
/// Handles key management, region selection, and feature toggles
/// </summary>
public interface IAzureConfigurationService
{
    /// <summary>
    /// Load Azure configuration from environment or user settings
    /// </summary>
    Task<AzureConfiguration> LoadConfigurationAsync();

    /// <summary>
    /// Save Azure configuration (for settings page)
    /// </summary>
    Task SaveConfigurationAsync(AzureConfiguration configuration);

    /// <summary>
    /// Check if Azure services are properly configured
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Get available Azure regions for speech services
    /// </summary>
    IEnumerable<AzureRegion> GetAvailableRegions();
}

public class AzureConfigurationService : IAzureConfigurationService
{
    private readonly ILogger<AzureConfigurationService> _logger;
    private AzureConfiguration? _cachedConfiguration;

    public bool IsConfigured => _cachedConfiguration?.IsValid == true;

    public AzureConfigurationService(ILogger<AzureConfigurationService> logger)
    {
        _logger = logger;
    }

    public async Task<AzureConfiguration> LoadConfigurationAsync()
    {
        if (_cachedConfiguration != null)
            return _cachedConfiguration;

        // Try environment variables first
        var config = new AzureConfiguration
        {
            SpeechSubscriptionKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY"),
            SpeechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION") ?? "eastus",
            EnableSpeechRecognition = true,
            EnableTextToSpeech = true,
            UseAviationPhoneology = true,
            DefaultVoiceProfile = ATCVoiceProfile.ControllerMale
        };

        // Try to load from local storage in a real app
        // For now, use environment variables or defaults
        
        _cachedConfiguration = config;
        
        if (config.IsValid)
        {
            _logger.LogInformation("Azure configuration loaded successfully for region: {Region}", config.SpeechRegion);
        }
        else
        {
            _logger.LogInformation("No Azure keys found, Web Speech API will be used as fallback");
        }

        return config;
    }

    public async Task SaveConfigurationAsync(AzureConfiguration configuration)
    {
        _cachedConfiguration = configuration;
        
        // In a real application, save to local storage or user preferences
        _logger.LogInformation("Azure configuration saved");
        
        await Task.CompletedTask;
    }

    public IEnumerable<AzureRegion> GetAvailableRegions()
    {
        return new[]
        {
            new AzureRegion { Code = "eastus", Name = "East US", Location = "Virginia" },
            new AzureRegion { Code = "eastus2", Name = "East US 2", Location = "Virginia" },
            new AzureRegion { Code = "westus", Name = "West US", Location = "California" },
            new AzureRegion { Code = "westus2", Name = "West US 2", Location = "Washington" },
            new AzureRegion { Code = "centralus", Name = "Central US", Location = "Iowa" },
            new AzureRegion { Code = "northcentralus", Name = "North Central US", Location = "Illinois" },
            new AzureRegion { Code = "southcentralus", Name = "South Central US", Location = "Texas" },
            new AzureRegion { Code = "westcentralus", Name = "West Central US", Location = "Wyoming" },
            new AzureRegion { Code = "canadacentral", Name = "Canada Central", Location = "Toronto" },
            new AzureRegion { Code = "canadaeast", Name = "Canada East", Location = "Quebec" },
            new AzureRegion { Code = "northeurope", Name = "North Europe", Location = "Ireland" },
            new AzureRegion { Code = "westeurope", Name = "West Europe", Location = "Netherlands" },
            new AzureRegion { Code = "uksouth", Name = "UK South", Location = "London" },
            new AzureRegion { Code = "ukwest", Name = "UK West", Location = "Cardiff" },
            new AzureRegion { Code = "francecentral", Name = "France Central", Location = "Paris" },
            new AzureRegion { Code = "germanynorth", Name = "Germany North", Location = "Berlin" },
            new AzureRegion { Code = "norwayeast", Name = "Norway East", Location = "Oslo" },
            new AzureRegion { Code = "switzerlandnorth", Name = "Switzerland North", Location = "Zurich" },
            new AzureRegion { Code = "australiaeast", Name = "Australia East", Location = "Sydney" },
            new AzureRegion { Code = "australiasoutheast", Name = "Australia Southeast", Location = "Melbourne" },
            new AzureRegion { Code = "southeastasia", Name = "Southeast Asia", Location = "Singapore" },
            new AzureRegion { Code = "eastasia", Name = "East Asia", Location = "Hong Kong" },
            new AzureRegion { Code = "japaneast", Name = "Japan East", Location = "Tokyo" },
            new AzureRegion { Code = "japanwest", Name = "Japan West", Location = "Osaka" },
            new AzureRegion { Code = "koreacentral", Name = "Korea Central", Location = "Seoul" },
            new AzureRegion { Code = "koreasouth", Name = "Korea South", Location = "Busan" },
            new AzureRegion { Code = "centralindia", Name = "Central India", Location = "Pune" },
            new AzureRegion { Code = "southindia", Name = "South India", Location = "Chennai" },
            new AzureRegion { Code = "westindia", Name = "West India", Location = "Mumbai" },
            new AzureRegion { Code = "brazilsouth", Name = "Brazil South", Location = "São Paulo" },
            new AzureRegion { Code = "southafricanorth", Name = "South Africa North", Location = "Johannesburg" }
        };
    }
}

/// <summary>
/// Azure Cognitive Services configuration
/// </summary>
public class AzureConfiguration
{
    public string? SpeechSubscriptionKey { get; set; }
    public string SpeechRegion { get; set; } = "eastus";
    public bool EnableSpeechRecognition { get; set; } = true;
    public bool EnableTextToSpeech { get; set; } = true;
    public bool UseAviationPhoneology { get; set; } = true;
    public ATCVoiceProfile DefaultVoiceProfile { get; set; } = ATCVoiceProfile.ControllerMale;
    public float RecognitionConfidenceThreshold { get; set; } = 0.7f;
    public bool EnableContinuousRecognition { get; set; } = true;
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Check if the configuration has valid Azure credentials
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(SpeechSubscriptionKey) 
                          && !string.IsNullOrWhiteSpace(SpeechRegion);

    /// <summary>
    /// Check if speech recognition is enabled and configured
    /// </summary>
    public bool CanUseSpeechRecognition => EnableSpeechRecognition && IsValid;

    /// <summary>
    /// Check if text-to-speech is enabled and configured
    /// </summary>
    public bool CanUseTextToSpeech => EnableTextToSpeech && IsValid;
}

/// <summary>
/// Available Azure regions for speech services
/// </summary>
public class AzureRegion
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public override string ToString() => $"{Name} ({Location})";
}

/// <summary>
/// Speech service performance metrics
/// </summary>
public class SpeechMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ServiceType { get; set; } = string.Empty; // "Azure" or "WebSpeech"
    public TimeSpan ResponseTime { get; set; }
    public float Confidence { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int AudioDurationMs { get; set; }
}