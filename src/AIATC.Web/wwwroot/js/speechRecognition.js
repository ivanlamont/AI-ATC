window.speechRecognition = {
    recognition: null,
    dotNetRef: null,
    isSupported: false,
    permissionGranted: false,

    // Azure fallback state (used when Web Speech API is unavailable)
    _azureMode: false,
    _azureToken: null,
    _azureRegion: null,
    _mediaRecorder: null,
    _audioChunks: [],
    _activeStream: null,

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;

        // Check browser support
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

        if (!SpeechRecognition) {
            console.warn('Web Speech API not supported in this browser — Azure fallback may be used');
            return false;
        }

        this.isSupported = true;

        // Create recognition instance
        this.recognition = new SpeechRecognition();

        // Configure recognition
        this.recognition.continuous = true;  // Keep listening
        this.recognition.interimResults = false;  // Only final results
        this.recognition.lang = 'en-US';
        this.recognition.maxAlternatives = 1;

        // Handle results
        this.recognition.onresult = (event) => {
            const last = event.results.length - 1;
            const transcript = event.results[last][0].transcript.trim();

            console.log('Recognized:', transcript);

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechRecognized', transcript);
            }
        };

        // Handle errors
        this.recognition.onerror = (event) => {
            console.error('Speech recognition error:', event.error);

            // Handle permission denied
            if (event.error === 'not-allowed') {
                this.permissionGranted = false;
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnPermissionDenied');
                }
            }

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
            }
        };

        // Handle start
        this.recognition.onstart = () => {
            console.log('Speech recognition started');
            this.permissionGranted = true;

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnListeningStarted');
            }
        };

        // Handle end
        this.recognition.onend = () => {
            console.log('Speech recognition ended');

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnListeningStopped');
            }
        };

        return true;
    },

    // Initialize Azure Speech fallback for browsers without Web Speech API.
    // dotNetRef must have been set by a prior initialize() call.
    initializeAzure: function (token, region) {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            console.warn('getUserMedia not supported — no speech recognition available');
            return false;
        }
        if (typeof MediaRecorder === 'undefined') {
            console.warn('MediaRecorder not supported — no speech recognition available');
            return false;
        }
        this._azureMode = true;
        this._azureToken = token;
        this._azureRegion = region;
        this.isSupported = true;
        console.log('Azure Speech fallback initialized for region:', region);
        return true;
    },

    requestPermission: async function () {
        try {
            // Check current permission status
            if (navigator.permissions && navigator.permissions.query) {
                const permissionStatus = await navigator.permissions.query({ name: 'microphone' });
                console.log('Microphone permission:', permissionStatus.state);

                if (permissionStatus.state === 'denied') {
                    return false;
                }

                // Listen for permission changes
                permissionStatus.addEventListener('change', () => {
                    console.log('Microphone permission changed to:', permissionStatus.state);
                    if (permissionStatus.state === 'denied') {
                        this.permissionGranted = false;
                        if (this.dotNetRef) {
                            this.dotNetRef.invokeMethodAsync('OnPermissionDenied');
                        }
                    }
                });
            }

            // Try to get user media to trigger permission prompt
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            stream.getTracks().forEach(track => track.stop()); // Stop immediately

            this.permissionGranted = true;
            return true;
        } catch (error) {
            console.error('Permission request failed:', error);
            this.permissionGranted = false;
            return false;
        }
    },

    hasPermission: function () {
        return this.permissionGranted;
    },

    start: function () {
        if (this._azureMode) {
            this._startAzureRecording();
            return;
        }

        if (!this.isSupported || !this.recognition) {
            console.error('Speech recognition not initialized');
            return;
        }

        try {
            this.recognition.start();
        } catch (e) {
            // Already started, ignore
            console.warn('Recognition already started');
        }
    },

    stop: function () {
        if (this._azureMode) {
            this._stopAzureRecording();
            return;
        }

        if (!this.isSupported || !this.recognition) {
            return;
        }

        try {
            this.recognition.stop();
        } catch (e) {
            console.warn('Error stopping recognition:', e);
        }
    },

    // ── Azure MediaRecorder fallback ──────────────────────────────────────────

    _startAzureRecording: async function () {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    sampleRate: 16000,
                    channelCount: 1,
                    echoCancellation: true,
                    noiseSuppression: true
                }
            });

            this._activeStream = stream;
            this._audioChunks = [];
            this.permissionGranted = true;

            // Pick a MIME type the browser supports
            const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
                ? 'audio/webm;codecs=opus'
                : MediaRecorder.isTypeSupported('audio/webm')
                    ? 'audio/webm'
                    : '';

            this._mediaRecorder = mimeType
                ? new MediaRecorder(stream, { mimeType })
                : new MediaRecorder(stream);

            this._mediaRecorder.ondataavailable = (e) => {
                if (e.data.size > 0) this._audioChunks.push(e.data);
            };

            this._mediaRecorder.onstop = () => this._processAzureAudio();
            this._mediaRecorder.start();

            console.log('Azure recording started with MIME:', this._mediaRecorder.mimeType);
            if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnListeningStarted');

        } catch (err) {
            console.error('Failed to start Azure recording:', err);
            if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                this.permissionGranted = false;
                if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnPermissionDenied');
            } else {
                if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnSpeechError', err.message);
            }
        }
    },

    _stopAzureRecording: function () {
        if (this._mediaRecorder && this._mediaRecorder.state !== 'inactive') {
            this._mediaRecorder.stop();
        }
        // OnListeningStopped is fired here; OnSpeechRecognized fires async after audio processing
        if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('OnListeningStopped');
    },

    _processAzureAudio: async function () {
        // Release the microphone
        if (this._activeStream) {
            this._activeStream.getTracks().forEach(t => t.stop());
            this._activeStream = null;
        }

        if (this._audioChunks.length === 0) {
            console.warn('Azure recording: no audio chunks captured');
            return;
        }

        try {
            const mimeType = this._mediaRecorder ? this._mediaRecorder.mimeType : 'audio/webm';
            const audioBlob = new Blob(this._audioChunks, { type: mimeType });
            this._audioChunks = [];

            // Convert to PCM WAV (azureSpeech.js provides window.convertToWav)
            if (typeof window.convertToWav !== 'function') {
                console.error('window.convertToWav not available — ensure azureSpeech.js is loaded');
                return;
            }
            const wavData = await window.convertToWav(audioBlob);
            if (!wavData || wavData.length === 0) {
                console.warn('WAV conversion returned empty data');
                return;
            }

            const endpoint =
                `https://${this._azureRegion}.stt.speech.microsoft.com` +
                `/speech/recognition/conversation/cognitiveservices/v1` +
                `?language=en-US&format=detailed`;

            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this._azureToken}`,
                    'Content-Type': 'audio/wav'
                },
                body: wavData
            });

            if (response.ok) {
                const result = await response.json();
                console.log('Azure STT result:', result.RecognitionStatus, result.DisplayText);
                if (result.RecognitionStatus === 'Success' && result.DisplayText) {
                    if (this.dotNetRef) {
                        this.dotNetRef.invokeMethodAsync('OnSpeechRecognized', result.DisplayText);
                    }
                } else if (result.RecognitionStatus === 'NoMatch') {
                    console.log('Azure STT: no speech detected');
                }
            } else {
                const errText = await response.text();
                console.error('Azure STT request failed:', response.status, errText);
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnSpeechError', `Azure STT error: ${response.status}`);
                }
            }
        } catch (err) {
            console.error('Azure STT processing error:', err);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', err.message);
            }
        }
    }
};
