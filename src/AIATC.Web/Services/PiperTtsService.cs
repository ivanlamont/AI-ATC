using Serilog;

namespace AIATC.Web.Services;

/// <summary>
/// Client for the Piper TTS service, proxied through the BFF at /api/speech/synthesize.
/// Returns WAV audio bytes for a given text and Piper voice name.
/// </summary>
public interface IPiperTtsService
{
    /// <summary>
    /// Synthesize speech using Piper TTS via the BFF proxy.
    /// </summary>
    /// <param name="text">The text to synthesize (aviation phoneology is applied before sending).</param>
    /// <param name="piperVoice">Piper voice model name, e.g. "en_US-ryan-high".</param>
    /// <returns>WAV audio bytes, or empty array on failure.</returns>
    Task<byte[]> SynthesizeSpeechAsync(string text, string piperVoice);

    /// <summary>
    /// Check whether the Piper TTS backend is reachable.
    /// </summary>
    Task<bool> IsAvailableAsync();
}

public class PiperTtsService : IPiperTtsService
{
    private readonly HttpClient _httpClient;

    public PiperTtsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text, string piperVoice)
    {
        try
        {
            var enhancedText = AviationPhoneology.Enhance(text);

            var url = $"api/speech/synthesize?voice={Uri.EscapeDataString(piperVoice)}";
            var content = new StringContent(enhancedText, System.Text.Encoding.UTF8, "text/plain");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            Log.Error("Piper TTS synthesis failed: {Status} {Reason}",
                (int)response.StatusCode, response.ReasonPhrase);
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during Piper TTS synthesis");
            return Array.Empty<byte>();
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/speech/piper-status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Aviation-specific text enhancements for TTS pronunciation.
/// Extracted from AzureSpeechService so both Azure and Piper paths can reuse it.
/// Plain text version (no SSML tags) suitable for Piper.
/// </summary>
public static class AviationPhoneology
{
    public static string Enhance(string text)
    {
        var enhanced = text;

        // Replace digits with aviation-specific pronunciations
        enhanced = enhanced
            .Replace("0", " zero ")
            .Replace("1", " one ")
            .Replace("2", " two ")
            .Replace("3", " tree ")   // Aviation pronunciation
            .Replace("4", " four ")
            .Replace("5", " five ")
            .Replace("6", " six ")
            .Replace("7", " seven ")
            .Replace("8", " eight ")
            .Replace("9", " niner "); // Aviation pronunciation

        // Clean up extra spaces
        while (enhanced.Contains("  "))
            enhanced = enhanced.Replace("  ", " ");

        return enhanced.Trim();
    }
}
