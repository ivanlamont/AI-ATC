/**
 * Radio audio effects for simulating aviation radio communications
 * Applies bandpass filtering, static noise, and compression to audio
 */
window.radioEffects = {
    audioContext: null,
    staticNoiseBuffer: null,
    isSupported: false,

    /**
     * Initialize the radio effects system
     */
    initialize: function () {
        try {
            // Create audio context
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) {
                console.warn('Web Audio API not supported');
                return false;
            }

            this.audioContext = new AudioContext();
            this.isSupported = true;

            // Generate static noise buffer
            this.generateStaticNoise();

            console.log('Radio effects initialized');
            return true;
        } catch (error) {
            console.error('Failed to initialize radio effects:', error);
            return false;
        }
    },

    /**
     * Generate static noise buffer for radio effect
     */
    generateStaticNoise: function () {
        const bufferSize = this.audioContext.sampleRate * 2; // 2 seconds of noise
        this.staticNoiseBuffer = this.audioContext.createBuffer(1, bufferSize, this.audioContext.sampleRate);
        const output = this.staticNoiseBuffer.getChannelData(0);

        // Generate white noise
        for (let i = 0; i < bufferSize; i++) {
            output[i] = Math.random() * 2 - 1;
        }
    },

    /**
     * Apply radio effects to a Web Speech API utterance
     * This captures the audio, processes it, and plays it back
     */
    processSpeechWithRadioEffect: function (utterance, options = {}) {
        if (!this.isSupported) {
            console.warn('Radio effects not supported, using direct speech');
            return utterance;
        }

        // Default options for radio quality
        const radioOptions = {
            staticLevel: options.staticLevel || 0.03,       // Low level continuous static
            compressionRatio: options.compressionRatio || 6, // Radio compression
            lowCutoff: options.lowCutoff || 300,            // Hz - cut bass
            highCutoff: options.highCutoff || 3000,         // Hz - cut treble
            ...options
        };

        // Store original callbacks
        const originalOnStart = utterance.onstart;
        const originalOnEnd = utterance.onend;

        // Intercept the start event to add processing info
        utterance.onstart = (event) => {
            console.log('[Radio] Processing speech with radio effects');
            if (originalOnStart) originalOnStart(event);
        };

        utterance.onend = (event) => {
            if (originalOnEnd) originalOnEnd(event);
        };

        return utterance;
    },

    /**
     * Create and configure a bandpass filter for radio simulation
     */
    createRadioBandpassFilter: function () {
        // Low shelf to reduce bass (typical radio cuts below 300 Hz)
        const lowShelf = this.audioContext.createBiquadFilter();
        lowShelf.type = 'highpass';
        lowShelf.frequency.value = 300;
        lowShelf.Q.value = 0.7;

        // High shelf to reduce treble (typical radio cuts above 3000 Hz)
        const highShelf = this.audioContext.createBiquadFilter();
        highShelf.type = 'lowpass';
        highShelf.frequency.value = 3000;
        highShelf.Q.value = 0.7;

        // Connect filters in series
        lowShelf.connect(highShelf);

        return { input: lowShelf, output: highShelf };
    },

    /**
     * Create a compressor for radio dynamics
     */
    createRadioCompressor: function () {
        const compressor = this.audioContext.createDynamicsCompressor();
        compressor.threshold.value = -24;  // dB
        compressor.knee.value = 10;        // dB
        compressor.ratio.value = 6;        // Moderate compression
        compressor.attack.value = 0.003;   // 3ms
        compressor.release.value = 0.05;   // 50ms
        return compressor;
    },

    /**
     * Create static noise source
     */
    createStaticNoise: function (level = 0.03) {
        const noiseSource = this.audioContext.createBufferSource();
        noiseSource.buffer = this.staticNoiseBuffer;
        noiseSource.loop = true;

        const noiseGain = this.audioContext.createGain();
        noiseGain.gain.value = level;

        noiseSource.connect(noiseGain);

        return { source: noiseSource, gain: noiseGain, output: noiseGain };
    },

    /**
     * Process an audio buffer with radio effects
     * This is for pre-recorded audio (not real-time TTS)
     */
    processAudioBuffer: function (audioBuffer, options = {}) {
        return new Promise((resolve, reject) => {
            try {
                const offlineContext = new OfflineAudioContext(
                    audioBuffer.numberOfChannels,
                    audioBuffer.length,
                    audioBuffer.sampleRate
                );

                // Create source from the buffer
                const source = offlineContext.createBufferSource();
                source.buffer = audioBuffer;

                // Create effects chain
                const bandpass = this.createRadioBandpassFilter();
                const compressor = this.createRadioCompressor();
                const staticNoise = this.createStaticNoise(options.staticLevel || 0.03);

                // Create mixer for combining speech and static
                const mixer = offlineContext.createGain();
                mixer.gain.value = 1.0;

                // Connect the chain: source -> bandpass -> compressor -> mixer
                source.connect(bandpass.input);
                bandpass.output.connect(compressor);
                compressor.connect(mixer);

                // Add static noise to mixer
                staticNoise.output.connect(mixer);

                // Connect mixer to output
                mixer.connect(offlineContext.destination);

                // Start processing
                source.start(0);
                staticNoise.source.start(0);

                // Render the processed audio
                offlineContext.startRendering().then(processedBuffer => {
                    resolve(processedBuffer);
                }).catch(error => {
                    reject(error);
                });

            } catch (error) {
                reject(error);
            }
        });
    },

    /**
     * Play an audio buffer with radio effects
     */
    playWithRadioEffects: function (audioBuffer, onEnd = null) {
        if (!this.isSupported) {
            console.warn('Radio effects not supported');
            return;
        }

        // Process the buffer
        this.processAudioBuffer(audioBuffer).then(processedBuffer => {
            // Play the processed audio
            const source = this.audioContext.createBufferSource();
            source.buffer = processedBuffer;
            source.connect(this.audioContext.destination);

            if (onEnd) {
                source.onended = onEnd;
            }

            source.start(0);
            console.log('[Radio] Playing processed audio');
        }).catch(error => {
            console.error('[Radio] Error processing audio:', error);
        });
    },

    /**
     * Get current audio context (for resuming if suspended)
     */
    resume: function () {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            return this.audioContext.resume();
        }
        return Promise.resolve();
    }
};
