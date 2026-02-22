using System.Text;
using System.Text.Json;
using Serilog;

namespace AIATC.Web.Services;

/// <summary>
/// Enhanced speech service that integrates Azure Cognitive Services with Web Speech API fallback
/// Provides professional aviation phraseology recognition and synthesis
/// </summary>
public interface IAzureSpeechService
{
    /// <summary>
    /// Initialize the speech service with Azure credentials
    /// </summary>
    Task<bool> InitializeAsync(string? subscriptionKey = null, string? region = null);

    /// <summary>
    /// Convert speech to text with aviation phraseology optimization
    /// </summary>
    Task<SpeechRecognitionResult> RecognizeSpeechAsync(byte[] audioData, string language = "en-US");

    /// <summary>
    /// Convert text to speech with ATC voice characteristics
    /// </summary>
    Task<byte[]> SynthesizeSpeechAsync(string text, ATCVoiceProfile voiceProfile = ATCVoiceProfile.ControllerMale);

    /// <summary>
    /// Start continuous speech recognition for live ATC communications
    /// </summary>
    Task<bool> StartContinuousRecognitionAsync();

    /// <summary>
    /// Stop continuous speech recognition
    /// </summary>
    Task StopContinuousRecognitionAsync();

    /// <summary>
    /// Returns the current speech token and region if available, fetching from the BFF if needed.
    /// Returns null if Azure Speech is not configured.
    /// </summary>
    Task<(string Token, string Region)?> GetOrFetchTokenAsync();

    /// <summary>
    /// Check if aviation vocabulary is loaded
    /// </summary>
    bool IsVocabularyLoaded { get; }

    /// <summary>
    /// Get vocabulary statistics if available
    /// </summary>
    VocabularyStats? GetVocabularyStats();

    /// <summary>
    /// Event fired when speech is recognized
    /// </summary>
    event EventHandler<SpeechRecognitionResult>? SpeechRecognized;

    /// <summary>
    /// Event fired when recognition confidence is low
    /// </summary>
    event EventHandler<string>? RecognitionUncertain;
}

/// <summary>
/// Implementation of Azure-enhanced speech service
/// </summary>
public class AzureSpeechService : IAzureSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly SpeechRecognitionService _fallbackRecognition;
    private readonly TextToSpeechService _fallbackTTS;
    private readonly AviationVocabularyService _vocabularyService;

    private string? _speechToken;
    private string? _region;
    private DateTimeOffset _tokenFetchedAt = DateTimeOffset.MinValue;
    private static readonly TimeSpan TokenRefreshInterval = TimeSpan.FromMinutes(9);
    private bool _isAzureInitialized;
    private bool _isListening;

    public bool IsAzureAvailable => !string.IsNullOrEmpty(_speechToken) && _isAzureInitialized;
    public bool IsVocabularyLoaded => _vocabularyService.IsLoaded;

    /// <summary>
    /// Get vocabulary statistics
    /// </summary>
    public VocabularyStats? GetVocabularyStats() => _vocabularyService.IsLoaded ? _vocabularyService.GetStats() : null;

    /// <inheritdoc />
    public async Task<(string Token, string Region)?> GetOrFetchTokenAsync()
    {
        if (IsAzureAvailable)
            return (_speechToken!, _region ?? "eastus");

        var fetched = await FetchSpeechTokenFromBffAsync();
        if (fetched && !string.IsNullOrEmpty(_speechToken))
            return (_speechToken, _region ?? "eastus");

        return null;
    }

    public event EventHandler<SpeechRecognitionResult>? SpeechRecognized;
    public event EventHandler<string>? RecognitionUncertain;

    public AzureSpeechService(
        HttpClient httpClient,
        SpeechRecognitionService fallbackRecognition,
        TextToSpeechService fallbackTTS,
        AviationVocabularyService vocabularyService)
    {
        _httpClient = httpClient;
        _fallbackRecognition = fallbackRecognition;
        _fallbackTTS = fallbackTTS;
        _vocabularyService = vocabularyService;
        
        // Wire up fallback events
        _fallbackRecognition.SpeechRecognized += OnFallbackSpeechRecognized;
    }

    public async Task<bool> InitializeAsync(string? subscriptionKey = null, string? region = null)
    {
        try
        {
            // Load aviation vocabulary first
            var vocabularyLoaded = await _vocabularyService.LoadVocabularyAsync();
            if (vocabularyLoaded)
            {
                var stats = _vocabularyService.GetStats();
                Log.Information($"Aviation vocabulary loaded: {stats.TotalTerms} terms, {stats.ControllerVerbs} controller verbs, {stats.PilotResponses} pilot responses");
            }
            else
            {
                Log.Warning("Failed to load aviation vocabulary, proceeding without custom terms");
            }

            // Fetch a short-lived token from the BFF (subscription key stays server-side)
            var tokenFetched = await FetchSpeechTokenFromBffAsync();
            if (tokenFetched)
            {
                _isAzureInitialized = true;
                Log.Information("Azure Speech Services initialized via BFF token for region {Region}", _region);
                return true;
            }
            else
            {
                Log.Warning("BFF speech token unavailable, falling back to Web Speech API");
                return await InitializeFallbackAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize Azure Speech Services, using fallback");
            return await InitializeFallbackAsync();
        }
    }

    private async Task<bool> FetchSpeechTokenFromBffAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/speech/token", null);
            if (!response.IsSuccessStatusCode)
            {
                Log.Information("BFF speech token endpoint returned {Status}", response.StatusCode);
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();
            var r = doc.RootElement.GetProperty("region").GetString();

            if (string.IsNullOrWhiteSpace(token))
                return false;

            _speechToken = token;
            _region = r ?? "eastus";
            _tokenFetchedAt = DateTimeOffset.UtcNow;
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to fetch speech token from BFF");
            return false;
        }
    }

    private async Task EnsureTokenFreshAsync()
    {
        if (DateTimeOffset.UtcNow - _tokenFetchedAt >= TokenRefreshInterval)
        {
            await FetchSpeechTokenFromBffAsync();
        }
    }

    public async Task<SpeechRecognitionResult> RecognizeSpeechAsync(byte[] audioData, string language = "en-US")
    {
        if (IsAzureAvailable)
        {
            return await RecognizeWithAzureAsync(audioData, language);
        }
        else
        {
            // Fallback to Web Speech API (would need audio conversion)
            Log.Information("Using Web Speech API fallback for recognition");
            return new SpeechRecognitionResult
            {
                IsSuccess = false,
                Text = "",
                Confidence = 0,
                ErrorMessage = "Web Speech API requires live microphone input"
            };
        }
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text, ATCVoiceProfile voiceProfile = ATCVoiceProfile.ControllerMale)
    {
        if (IsAzureAvailable)
        {
            return await SynthesizeWithAzureAsync(text, voiceProfile);
        }
        else
        {
            // Fallback to Web Speech API
            Log.Information("Using Web Speech API fallback for synthesis");
            await _fallbackTTS.SpeakAsync(text);
            return Array.Empty<byte>(); // Web Speech API doesn't return audio data
        }
    }

    public async Task<bool> StartContinuousRecognitionAsync()
    {
        if (IsAzureAvailable)
        {
            return await StartAzureContinuousRecognitionAsync();
        }
        else
        {
            await _fallbackRecognition.StartListeningAsync();
            return true;
        }
    }

    public async Task StopContinuousRecognitionAsync()
    {
        if (IsAzureAvailable)
        {
            await StopAzureContinuousRecognitionAsync();
        }
        else
        {
            await _fallbackRecognition.StopListeningAsync();
        }
    }

    private async Task<bool> InitializeFallbackAsync()
    {
        var recognitionInit = await _fallbackRecognition.InitializeAsync();
        var ttsInit = await _fallbackTTS.InitializeAsync();
        return recognitionInit && ttsInit;
    }

    private async Task<SpeechRecognitionResult> RecognizeWithAzureAsync(byte[] audioData, string language)
    {
        try
        {
            await EnsureTokenFreshAsync();

            var endpoint = $"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1";
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _speechToken);
            request.Headers.Add("Transfer-Encoding", "chunked");
            request.Content = new ByteArrayContent(audioData);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

            // Enhanced query parameters with aviation vocabulary
            var queryParams = $"?language={language}&format=detailed&profanityAction=Removed";
            
            // Add custom vocabulary if available
            if (_vocabularyService.IsLoaded)
            {
                queryParams += "&wordLevelTimestamps=true"; // Better for aviation phraseology
                
                // Add custom vocabulary terms to improve recognition
                var vocabularyTerms = _vocabularyService.GetVocabularyTerms();
                if (vocabularyTerms.Count > 0)
                {
                    // Azure Speech supports phrase lists for better recognition
                    var phraseHints = string.Join(",", vocabularyTerms.Take(100)); // Limit to first 100 terms
                    request.Headers.Add("X-CustomSpeech-PhraseList", phraseHints);
                }
            }
            
            request.RequestUri = new Uri(endpoint + queryParams);

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = ParseAzureResponse(jsonResponse);
                
                // Post-process with aviation vocabulary if available
                if (_vocabularyService.IsLoaded && result.IsSuccess)
                {
                    result.Text = EnhanceRecognitionWithVocabulary(result.Text);
                }
                
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("Azure Speech recognition failed: {Error}", error);
                return new SpeechRecognitionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Azure API error: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during Azure speech recognition");
            return new SpeechRecognitionResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<byte[]> SynthesizeWithAzureAsync(string text, ATCVoiceProfile voiceProfile)
    {
        try
        {
            await EnsureTokenFreshAsync();

            var endpoint = $"https://{_region}.tts.speech.microsoft.com/cognitiveservices/v1";
            var ssml = CreateATCSSML(text, voiceProfile);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _speechToken);
            request.Headers.Add("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
            request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Error("Azure Speech synthesis failed: {Error}", error);
                return Array.Empty<byte>();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during Azure speech synthesis");
            return Array.Empty<byte>();
        }
    }

    private async Task<bool> StartAzureContinuousRecognitionAsync()
    {
        // This would implement WebSocket connection to Azure for real-time recognition
        // For now, return false to use fallback
        Log.Information("Azure continuous recognition not implemented, using fallback");
        await _fallbackRecognition.StartListeningAsync();
        return true;
    }

    private async Task StopAzureContinuousRecognitionAsync()
    {
        // This would close WebSocket connection
        await _fallbackRecognition.StopListeningAsync();
    }

    private string CreateATCSSML(string text, ATCVoiceProfile voiceProfile)
    {
        var voiceName = voiceProfile switch
        {
            ATCVoiceProfile.ControllerMale => "en-US-DavisNeural",
            ATCVoiceProfile.ControllerFemale => "en-US-JennyNeural", 
            ATCVoiceProfile.PilotMale => "en-US-BrandonNeural",
            ATCVoiceProfile.PilotFemale => "en-US-AriaNeural",
            _ => "en-US-DavisNeural"
        };

        // Enhance text with aviation-specific pronunciations from vocabulary
        var enhancedText = EnhanceAviationPhoneology(text);

        return $@"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
            <voice name='{voiceName}'>
                <prosody rate='0.9' pitch='medium'>
                    {enhancedText}
                </prosody>
            </voice>
        </speak>";
    }

    private string EnhanceAviationPhoneology(string text)
    {
        var enhanced = text;
        
        // Apply vocabulary-based pronunciation dictionary if available
        if (_vocabularyService.IsLoaded)
        {
            var pronunciationDict = _vocabularyService.GetPronunciationDictionary();
            foreach (var kvp in pronunciationDict)
            {
                enhanced = enhanced.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Replace numbers with aviation-specific pronunciations
        enhanced = enhanced
            .Replace("0", "zero")
            .Replace("1", "one") 
            .Replace("2", "two")
            .Replace("3", "tree")  // Aviation pronunciation
            .Replace("4", "four")
            .Replace("5", "five")
            .Replace("6", "six")
            .Replace("7", "seven")
            .Replace("8", "eight")
            .Replace("9", "niner"); // Aviation pronunciation

        // Add aviation phraseology enhancements with SSML breaks
        enhanced = enhanced.Replace("Roger", "<break time='200ms'/>Roger<break time='200ms'/>");
        enhanced = enhanced.Replace("Wilco", "<break time='200ms'/>Wilco<break time='200ms'/>");
        enhanced = enhanced.Replace("Affirmative", "<break time='150ms'/>Affirmative<break time='150ms'/>");
        enhanced = enhanced.Replace("Negative", "<break time='150ms'/>Negative<break time='150ms'/>");
        enhanced = enhanced.Replace("Unable", "<break time='150ms'/>Unable<break time='150ms'/>");

        // Add emphasis for critical commands
        enhanced = enhanced.Replace("CLEARED FOR TAKEOFF", "<emphasis level='strong'>CLEARED FOR TAKEOFF</emphasis>");
        enhanced = enhanced.Replace("CLEARED TO LAND", "<emphasis level='strong'>CLEARED TO LAND</emphasis>");
        enhanced = enhanced.Replace("HOLD SHORT", "<emphasis level='strong'>HOLD SHORT</emphasis>");

        return enhanced;
    }

    private string EnhanceRecognitionWithVocabulary(string recognizedText)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || !_vocabularyService.IsLoaded)
            return recognizedText;

        var enhanced = recognizedText;

        // Apply common aviation phraseology corrections
        var vocabularyTerms = _vocabularyService.GetVocabularyTerms();
        
        // Correct common speech-to-text errors in aviation context
        var commonCorrections = new Dictionary<string, string>
        {
            { "clear for takeoff", "CLEARED FOR TAKEOFF" },
            { "clear to land", "CLEARED TO LAND" },
            { "turn left heading", "TURN LEFT HEADING" },
            { "turn right heading", "TURN RIGHT HEADING" },
            { "climb and maintain", "CLIMB AND MAINTAIN" },
            { "descend and maintain", "DESCEND AND MAINTAIN" },
            { "contact departure", "CONTACT DEPARTURE" },
            { "contact approach", "CONTACT APPROACH" },
            { "hold short", "HOLD SHORT" },
            { "taxi to", "TAXI TO" },
            { "unable", "UNABLE" },
            { "wilco", "WILCO" },
            { "affirmative", "AFFIRMATIVE" },
            { "negative", "NEGATIVE" }
        };

        foreach (var correction in commonCorrections)
        {
            enhanced = enhanced.Replace(correction.Key, correction.Value, StringComparison.OrdinalIgnoreCase);
        }

        // Ensure aviation terms are properly capitalized
        foreach (var term in vocabularyTerms)
        {
            if (term.Length > 2) // Skip single letters
            {
                enhanced = System.Text.RegularExpressions.Regex.Replace(
                    enhanced, 
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(term)}\b", 
                    term, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }

        Log.Debug($"Enhanced recognition: '{recognizedText}' -> '{enhanced}'");
        return enhanced;
    }

    private SpeechRecognitionResult ParseAzureResponse(string jsonResponse)
    {
        try
        {
            var response = JsonSerializer.Deserialize<AzureSpeechResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response?.RecognitionStatus == "Success" && response.DisplayText != null)
            {
                return new SpeechRecognitionResult
                {
                    IsSuccess = true,
                    Text = response.DisplayText,
                    Confidence = response.NBest?[0]?.Confidence ?? 0.8f,
                    AviationCommand = ParseAviationCommand(response.DisplayText)
                };
            }
            else
            {
                return new SpeechRecognitionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Recognition failed: {response?.RecognitionStatus}"
                };
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse Azure response");
            return new SpeechRecognitionResult
            {
                IsSuccess = false,
                ErrorMessage = "Failed to parse response"
            };
        }
    }

    private AviationCommand? ParseAviationCommand(string text)
    {
        // Simple command parsing - would be enhanced with NLP
        var lowerText = text.ToLower();
        
        if (lowerText.Contains("turn") && lowerText.Contains("heading"))
        {
            return new AviationCommand
            {
                Type = CommandType.Heading,
                Text = text,
                Confidence = 0.85f
            };
        }
        else if (lowerText.Contains("climb") || lowerText.Contains("descend"))
        {
            return new AviationCommand
            {
                Type = CommandType.Altitude,
                Text = text,
                Confidence = 0.85f
            };
        }
        else if (lowerText.Contains("direct"))
        {
            return new AviationCommand
            {
                Type = CommandType.Direct,
                Text = text,
                Confidence = 0.85f
            };
        }

        return null;
    }

    private void OnFallbackSpeechRecognized(object? sender, string recognizedText)
    {
        var result = new SpeechRecognitionResult
        {
            IsSuccess = true,
            Text = recognizedText,
            Confidence = 0.7f, // Lower confidence for fallback
            AviationCommand = ParseAviationCommand(recognizedText)
        };

        SpeechRecognized?.Invoke(this, result);
    }
}

/// <summary>
/// ATC voice profiles for different roles
/// </summary>
public enum ATCVoiceProfile
{
    ControllerMale,
    ControllerFemale,
    PilotMale,
    PilotFemale
}

/// <summary>
/// Speech recognition result with aviation enhancements
/// </summary>
public class SpeechRecognitionResult
{
    public bool IsSuccess { get; set; }
    public string Text { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public AviationCommand? AviationCommand { get; set; }
}

/// <summary>
/// Parsed aviation command from speech
/// </summary>
public class AviationCommand
{
    public CommandType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Types of aviation commands
/// </summary>
public enum CommandType
{
    Heading,
    Altitude,
    Speed,
    Direct,
    Contact,
    Approach,
    Hold,
    Unknown
}

// Azure API response models
internal class AzureSpeechResponse
{
    public string? RecognitionStatus { get; set; }
    public string? DisplayText { get; set; }
    public List<NBestItem>? NBest { get; set; }
}

internal class NBestItem
{
    public float Confidence { get; set; }
    public string? Display { get; set; }
}
