window.textToSpeech = {
    synth: null,
    voices: [],
    voiceMap: {},
    isSupported: false,
    radioEffectsEnabled: true,
    staticSource: null,
    staticGain: null,

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

        // Initialize radio effects if available
        if (window.radioEffects) {
            window.radioEffects.initialize();
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

        // Check if this should use radio effects (for pilot voices)
        const useRadioEffects = options.useRadioEffects !== undefined 
            ? options.useRadioEffects 
            : (options.voice === 'pilot' && this.radioEffectsEnabled);

        // Apply radio quality adjustments to the utterance itself
        if (useRadioEffects) {
            // Slightly reduce rate for radio clarity
            utterance.rate = (options.rate || 1.0) * 0.95;
            
            // Slightly lower pitch for radio transmission
            utterance.pitch = (options.pitch || 1.0) * 0.92;
            
            // Reduce volume slightly to simulate radio attenuation
            utterance.volume = (options.volume || 1.0) * 0.85;
        } else {
            // Normal speech parameters
            utterance.rate = options.rate || 1.0;
            utterance.pitch = options.pitch || 1.0;
            utterance.volume = options.volume || 1.0;
        }

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
            
            // Start static noise for radio effect
            if (useRadioEffects) {
                this.startRadioStatic(0.02); // Low level static
            }
        };

        utterance.onend = () => {
            console.log('TTS ended');
            
            // Stop static noise
            if (useRadioEffects) {
                this.stopRadioStatic();
            }
        };

        utterance.onerror = (event) => {
            console.error('TTS error:', event.error);
            
            // Make sure to stop static on error
            if (useRadioEffects) {
                this.stopRadioStatic();
            }
        };

        // Speak
        this.synth.speak(utterance);
    },

    /**
     * Start background static noise for radio effect
     */
    startRadioStatic: function (level = 0.02) {
        if (!window.radioEffects || !window.radioEffects.isSupported) {
            return;
        }

        try {
            // Resume audio context if needed
            window.radioEffects.resume();

            const context = window.radioEffects.audioContext;
            
            // Create white noise using an audio buffer
            const bufferSize = context.sampleRate * 0.5; // 0.5 seconds
            const buffer = context.createBuffer(1, bufferSize, context.sampleRate);
            const data = buffer.getChannelData(0);
            
            for (let i = 0; i < bufferSize; i++) {
                data[i] = (Math.random() * 2 - 1) * 0.3; // Reduced amplitude
            }
            
            // Create and configure the noise source
            this.staticSource = context.createBufferSource();
            this.staticSource.buffer = buffer;
            this.staticSource.loop = true;
            
            // Create gain node for volume control
            this.staticGain = context.createGain();
            this.staticGain.gain.value = level;
            
            // Apply bandpass filter to the static
            const lowpass = context.createBiquadFilter();
            lowpass.type = 'lowpass';
            lowpass.frequency.value = 3000; // Cut high frequencies
            
            const highpass = context.createBiquadFilter();
            highpass.type = 'highpass';
            highpass.frequency.value = 300; // Cut low frequencies
            
            // Connect the chain
            this.staticSource.connect(highpass);
            highpass.connect(lowpass);
            lowpass.connect(this.staticGain);
            this.staticGain.connect(context.destination);
            
            // Start the noise
            this.staticSource.start(0);
            
            console.log('[Radio] Static noise started');
        } catch (error) {
            console.error('[Radio] Error starting static:', error);
        }
    },

    /**
     * Stop background static noise
     */
    stopRadioStatic: function () {
        if (this.staticSource) {
            try {
                // Fade out the static
                if (this.staticGain && window.radioEffects && window.radioEffects.audioContext) {
                    const context = window.radioEffects.audioContext;
                    this.staticGain.gain.setValueAtTime(this.staticGain.gain.value, context.currentTime);
                    this.staticGain.gain.linearRampToValueAtTime(0, context.currentTime + 0.1);
                    
                    // Stop after fade
                    setTimeout(() => {
                        if (this.staticSource) {
                            this.staticSource.stop();
                            this.staticSource = null;
                            this.staticGain = null;
                        }
                    }, 150);
                } else {
                    this.staticSource.stop();
                    this.staticSource = null;
                    this.staticGain = null;
                }
                
                console.log('[Radio] Static noise stopped');
            } catch (error) {
                console.error('[Radio] Error stopping static:', error);
                this.staticSource = null;
                this.staticGain = null;
            }
        }
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
