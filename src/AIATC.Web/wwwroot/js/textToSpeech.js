window.textToSpeech = {
    synth: null,
    voices: [],
    voiceMap: {},
    isSupported: false,

    initialize: function () {
        // Check browser support
        if (!window.speechSynthesis) {
            console.warn('Text-to-speech not supported in this browser');
            return false;
        }

        this.isSupported = true;
        this.synth = window.speechSynthesis;

        // Load voices (may need delay for some browsers)
        this.loadVoices();

        // Handle voice changes
        if (speechSynthesis.onvoiceschanged !== undefined) {
            speechSynthesis.onvoiceschanged = () => this.loadVoices();
        }

        return true;
    },

    loadVoices: function () {
        this.voices = this.synth.getVoices();

        // Map voice types to specific voices
        this.voiceMap = {
            'controller': this.findBestVoice('male', 'en-US'),
            'pilot': this.findBestVoice('male', 'en-US', 1), // Different male voice
            'default': this.voices[0]
        };

        console.log('Loaded voices:', this.voices.length);
    },

    findBestVoice: function (gender, lang, index = 0) {
        // Try to find voice matching criteria
        const matching = this.voices.filter(v =>
            v.lang.startsWith(lang) &&
            (v.name.toLowerCase().includes(gender) || v.name.toLowerCase().includes('male'))
        );

        return matching[index] || this.voices[0];
    },

    getVoices: function () {
        return this.voices.map(v => ({
            name: v.name,
            lang: v.lang,
            localService: v.localService,
            default: v.default
        }));
    },

    speak: function (text, options) {
        if (!this.isSupported) {
            console.warn('TTS not supported');
            return;
        }

        // Create utterance
        const utterance = new SpeechSynthesisUtterance(text);

        // Apply options
        utterance.rate = options.rate || 1.0;
        utterance.pitch = options.pitch || 1.0;
        utterance.volume = options.volume || 1.0;

        // Select voice
        if (options.voice) {
            if (this.voiceMap[options.voice]) {
                utterance.voice = this.voiceMap[options.voice];
            } else {
                // Try to find by name
                const voice = this.voices.find(v => v.name === options.voice);
                if (voice) {
                    utterance.voice = voice;
                }
            }
        }

        // Add event handlers
        utterance.onstart = () => {
            console.log('TTS started:', text);
        };

        utterance.onend = () => {
            console.log('TTS ended');
        };

        utterance.onerror = (event) => {
            console.error('TTS error:', event.error);
        };

        // Speak
        this.synth.speak(utterance);
    },

    stop: function () {
        if (this.isSupported && this.synth.speaking) {
            this.synth.pause();
            this.synth.resume(); // Resume to trigger end
        }
    },

    cancel: function () {
        if (this.isSupported) {
            this.synth.cancel();
        }
    }
};
