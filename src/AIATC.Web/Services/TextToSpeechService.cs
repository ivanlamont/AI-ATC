using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIATC.Web.Services;

/// <summary>
/// Service for text-to-speech using Web Speech API
/// </summary>
public class TextToSpeechService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized;

    public TextToSpeechService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the text-to-speech service
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            var supported = await _jsRuntime.InvokeAsync<bool>("textToSpeech.initialize");
            _isInitialized = supported;
            return supported;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets available voices
    /// </summary>
    public async Task<List<VoiceInfo>> GetVoicesAsync()
    {
        if (!_isInitialized)
            return new List<VoiceInfo>();

        try
        {
            return await _jsRuntime.InvokeAsync<List<VoiceInfo>>("textToSpeech.getVoices");
        }
        catch
        {
            return new List<VoiceInfo>();
        }
    }

    /// <summary>
    /// Speaks the given text
    /// </summary>
    public async Task SpeakAsync(string text, SpeechOptions? options = null)
    {
        if (!_isInitialized || string.IsNullOrWhiteSpace(text))
            return;

        options ??= new SpeechOptions();

        try
        {
            await _jsRuntime.InvokeVoidAsync("textToSpeech.speak", text, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS error: {ex.Message}");
        }
    }

    /// <summary>
    /// Speaks pilot readback with appropriate voice
    /// </summary>
    public async Task SpeakPilotReadbackAsync(string callsign, string readback)
    {
        var text = $"{callsign}, {readback}";

        var options = new SpeechOptions
        {
            Rate = 1.0f,     // Normal speed
            Pitch = 1.0f,    // Normal pitch
            Volume = 0.9f,   // Slightly quieter than controller
            Voice = "pilot"  // Use pilot voice if configured
        };

        await SpeakAsync(text, options);
    }

    /// <summary>
    /// Speaks controller command
    /// </summary>
    public async Task SpeakControllerCommandAsync(string text)
    {
        var options = new SpeechOptions
        {
            Rate = 1.0f,
            Pitch = 1.0f,
            Volume = 1.0f,
            Voice = "controller"
        };

        await SpeakAsync(text, options);
    }

    /// <summary>
    /// Stops current speech
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isInitialized)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("textToSpeech.stop");
        }
        catch
        {
            // Ignore errors
        }
    }

    /// <summary>
    /// Cancels all queued speech
    /// </summary>
    public async Task CancelAsync()
    {
        if (!_isInitialized)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("textToSpeech.cancel");
        }
        catch
        {
            // Ignore errors
        }
    }
}

/// <summary>
/// Information about an available voice
/// </summary>
public class VoiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public bool LocalService { get; set; }
    public bool Default { get; set; }
}

/// <summary>
/// Options for speech synthesis
/// </summary>
public class SpeechOptions
{
    /// <summary>
    /// Speech rate (0.1 to 10, default 1)
    /// </summary>
    public float Rate { get; set; } = 1.0f;

    /// <summary>
    /// Speech pitch (0 to 2, default 1)
    /// </summary>
    public float Pitch { get; set; } = 1.0f;

    /// <summary>
    /// Speech volume (0 to 1, default 1)
    /// </summary>
    public float Volume { get; set; } = 1.0f;

    /// <summary>
    /// Voice identifier or type ("pilot", "controller", or voice name)
    /// </summary>
    public string? Voice { get; set; }
}
