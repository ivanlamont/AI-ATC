window.speechRecognition = {
    recognition: null,
    dotNetRef: null,
    isSupported: false,
    permissionGranted: false,

    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;

        // Check browser support
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

        if (!SpeechRecognition) {
            console.warn('Speech recognition not supported in this browser');
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
        if (!this.isSupported || !this.recognition) {
            return;
        }

        try {
            this.recognition.stop();
        } catch (e) {
            console.warn('Error stopping recognition:', e);
        }
    }
};
