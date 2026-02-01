using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace AIATC.Web.Services;

/// <summary>
/// Service for browser-based speech recognition using Web Speech API
/// </summary>
public class SpeechRecognitionService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<SpeechRecognitionService>? _dotNetReference;
    private bool _isListening;

    public event EventHandler<string>? SpeechRecognized;
    public event EventHandler<string>? SpeechError;
    public event EventHandler? ListeningStarted;
    public event EventHandler? ListeningStopped;

    public bool IsListening => _isListening;

    public SpeechRecognitionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the speech recognition service
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            var supported = await _jsRuntime.InvokeAsync<bool>(
                "speechRecognition.initialize",
                _dotNetReference
            );

            return supported;
        }
        catch (Exception ex)
        {
            SpeechError?.Invoke(this, $"Initialization failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Starts continuous speech recognition
    /// </summary>
    public async Task StartListeningAsync()
    {
        if (_isListening)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("speechRecognition.start");
            _isListening = true;
            ListeningStarted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            SpeechError?.Invoke(this, $"Failed to start listening: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops speech recognition
    /// </summary>
    public async Task StopListeningAsync()
    {
        if (!_isListening)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("speechRecognition.stop");
            _isListening = false;
            ListeningStopped?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            SpeechError?.Invoke(this, $"Failed to stop listening: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles listening on/off
    /// </summary>
    public async Task ToggleListeningAsync()
    {
        if (_isListening)
            await StopListeningAsync();
        else
            await StartListeningAsync();
    }

    /// <summary>
    /// Called from JavaScript when speech is recognized
    /// </summary>
    [JSInvokable]
    public void OnSpeechRecognized(string transcript)
    {
        SpeechRecognized?.Invoke(this, transcript);
    }

    /// <summary>
    /// Called from JavaScript when an error occurs
    /// </summary>
    [JSInvokable]
    public void OnSpeechError(string error)
    {
        SpeechError?.Invoke(this, error);
    }

    /// <summary>
    /// Called from JavaScript when listening starts
    /// </summary>
    [JSInvokable]
    public void OnListeningStarted()
    {
        _isListening = true;
        ListeningStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called from JavaScript when listening stops
    /// </summary>
    [JSInvokable]
    public void OnListeningStopped()
    {
        _isListening = false;
        ListeningStopped?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isListening)
        {
            await StopListeningAsync();
        }

        _dotNetReference?.Dispose();
    }
}
