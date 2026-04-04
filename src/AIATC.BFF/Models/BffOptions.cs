namespace AIATC.BFF.Models;

public class BffOAuthOptions
{
    public OAuthProviderBffOptions Azure { get; set; } = new();
    public OAuthProviderBffOptions Google { get; set; } = new();
}

public class OAuthProviderBffOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>Azure only — e.g. https://login.microsoftonline.com/common</summary>
    public string Authority { get; set; } = string.Empty;
    public string Scope { get; set; } = "openid profile email";
}

public class AzureSpeechBffOptions
{
    public string SubscriptionKey { get; set; } = string.Empty;
    public string Region { get; set; } = "eastus";
}

public class FlightAwareBffOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public class PiperTtsBffOptions
{
    /// <summary>
    /// Base URL of the Wyoming Piper HTTP API, e.g. http://piper-tts:5000
    /// </summary>
    public string BaseUrl { get; set; } = "http://piper-tts:5000";
}
