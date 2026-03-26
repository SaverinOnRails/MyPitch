class AudioProcessor extends AudioWorkletProcessor {
    constructor() {
        super();

        this.buffer = new Float32Array();
        this.port.onmessage = (event) => {
            const incoming = event.data.audioData;

            let newBuf = new Float32Array(this.buffer.length + incoming.length);
            newBuf.set(this.buffer, 0);
            newBuf.set(incoming, this.buffer.length);
            this.buffer = newBuf;
        };
    }

    process(inputs, outputs) {
        const output = outputs[0];
        const channel = output[0];

        if (this.buffer.length < channel.length * 2) {
            this.port.postMessage({ type: "needData" });
        }

        for (let i = 0; i < channel.length; i++) {
            channel[i] = i < this.buffer.length ? this.buffer[i] : 0;
        }
        this.buffer = this.buffer.slice(channel.length);

        return true;
    }
}

registerProcessor('audio-processor', AudioProcessor);
