// Azure Speech Services JavaScript support functions

let currentAudioElement = null;

// Play audio data from Azure Speech Services
window.playAudioData = function(base64AudioData, mimeType) {
    try {
        // Stop any currently playing audio
        stopAudio();
        
        // Create blob from base64 data
        const byteCharacters = atob(base64AudioData);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const audioBlob = new Blob([byteArray], { type: mimeType });
        
        // Create audio element and play
        const audioUrl = URL.createObjectURL(audioBlob);
        currentAudioElement = new Audio(audioUrl);
        
        currentAudioElement.onended = function() {
            URL.revokeObjectURL(audioUrl);
            currentAudioElement = null;
        };
        
        currentAudioElement.onerror = function(error) {
            console.error('Error playing audio:', error);
            URL.revokeObjectURL(audioUrl);
            currentAudioElement = null;
        };
        
        currentAudioElement.play();
        
    } catch (error) {
        console.error('Error playing audio data:', error);
    }
};

// Stop currently playing audio
window.stopAudio = function() {
    if (currentAudioElement) {
        currentAudioElement.pause();
        currentAudioElement.currentTime = 0;
        currentAudioElement = null;
    }
};

// Check microphone permission
window.checkMicrophonePermission = async function() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach(track => track.stop());
        return true;
    } catch (error) {
        console.warn('Microphone permission denied or not available:', error);
        return false;
    }
};

// Audio recording utilities for future use
class AudioRecorder {
    constructor() {
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.stream = null;
    }
    
    async startRecording() {
        try {
            this.stream = await navigator.mediaDevices.getUserMedia({ 
                audio: {
                    sampleRate: 16000, // Azure Speech Services optimal rate
                    channelCount: 1,   // Mono
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                } 
            });
            
            this.mediaRecorder = new MediaRecorder(this.stream, {
                mimeType: 'audio/webm;codecs=opus'
            });
            
            this.audioChunks = [];
            
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.audioChunks.push(event.data);
                }
            };
            
            this.mediaRecorder.start(100); // Collect data every 100ms
            return true;
            
        } catch (error) {
            console.error('Error starting recording:', error);
            return false;
        }
    }
    
    stopRecording() {
        return new Promise((resolve) => {
            if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
                this.mediaRecorder.onstop = () => {
                    const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm;codecs=opus' });
                    this.cleanup();
                    resolve(audioBlob);
                };
                this.mediaRecorder.stop();
            } else {
                resolve(null);
            }
        });
    }
    
    cleanup() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }
        this.mediaRecorder = null;
        this.audioChunks = [];
    }
}

// Global audio recorder instance
window.audioRecorder = new AudioRecorder();

// Convert audio blob to WAV format for Azure
window.convertToWav = async function(audioBlob) {
    try {
        const arrayBuffer = await audioBlob.arrayBuffer();
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
        
        // Convert to WAV
        const wavBuffer = audioBufferToWav(audioBuffer);
        return new Uint8Array(wavBuffer);
        
    } catch (error) {
        console.error('Error converting to WAV:', error);
        return null;
    }
};

// WAV conversion helper
function audioBufferToWav(buffer) {
    const length = buffer.length;
    const sampleRate = buffer.sampleRate;
    const channels = buffer.numberOfChannels;
    
    // Create WAV header
    const arrayBuffer = new ArrayBuffer(44 + length * 2);
    const view = new DataView(arrayBuffer);
    
    // WAV header
    writeString(view, 0, 'RIFF');
    view.setUint32(4, 36 + length * 2, true);
    writeString(view, 8, 'WAVE');
    writeString(view, 12, 'fmt ');
    view.setUint32(16, 16, true);
    view.setUint16(20, 1, true);
    view.setUint16(22, channels, true);
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, sampleRate * 2, true);
    view.setUint16(32, 2, true);
    view.setUint16(34, 16, true);
    writeString(view, 36, 'data');
    view.setUint32(40, length * 2, true);
    
    // Convert audio data
    const channelData = buffer.getChannelData(0);
    let offset = 44;
    for (let i = 0; i < length; i++) {
        const sample = Math.max(-1, Math.min(1, channelData[i]));
        view.setInt16(offset, sample * 0x7FFF, true);
        offset += 2;
    }
    
    return arrayBuffer;
}

function writeString(view, offset, string) {
    for (let i = 0; i < string.length; i++) {
        view.setUint8(offset + i, string.charCodeAt(i));
    }
}

// Audio visualization for speech recognition feedback
class AudioVisualizer {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas ? this.canvas.getContext('2d') : null;
        this.analyser = null;
        this.dataArray = null;
        this.animationId = null;
    }
    
    start(audioStream) {
        if (!this.ctx || !audioStream) return;
        
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        this.analyser = audioContext.createAnalyser();
        const source = audioContext.createMediaStreamSource(audioStream);
        source.connect(this.analyser);
        
        this.analyser.fftSize = 256;
        const bufferLength = this.analyser.frequencyBinCount;
        this.dataArray = new Uint8Array(bufferLength);
        
        this.draw();
    }
    
    draw() {
        this.animationId = requestAnimationFrame(() => this.draw());
        
        if (!this.analyser || !this.dataArray || !this.ctx) return;
        
        this.analyser.getByteFrequencyData(this.dataArray);
        
        this.ctx.fillStyle = 'rgb(240, 240, 240)';
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        const barWidth = (this.canvas.width / this.dataArray.length) * 2.5;
        let barHeight;
        let x = 0;
        
        for (let i = 0; i < this.dataArray.length; i++) {
            barHeight = this.dataArray[i] / 2;
            
            this.ctx.fillStyle = `rgb(50, ${barHeight + 100}, 50)`;
            this.ctx.fillRect(x, this.canvas.height - barHeight / 2, barWidth, barHeight);
            
            x += barWidth + 1;
        }
    }
    
    stop() {
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
            this.animationId = null;
        }
        if (this.ctx) {
            this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        }
    }
}

// Export for global use
window.AudioVisualizer = AudioVisualizer;

console.log('Azure Speech JavaScript utilities loaded');