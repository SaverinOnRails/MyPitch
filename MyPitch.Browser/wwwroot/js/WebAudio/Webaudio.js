var exports;
var runtime;
var audio_context;
var initedServer = false;
var blocksize = 1024;
export async function startAudioServer() {
    const { getAssemblyExports } = await globalThis.getDotnetRuntime(0);
    exports = await getAssemblyExports("MyPitch.Browser.dll");
    runtime = await globalThis.getDotnetRuntime(0);
    document.body.onclick = async function () {
        if (initedServer) return;
        console.log("Starting audio server once...")
        initedServer = true;
        audio_context = new AudioContext();
        await audio_context.audioWorklet.addModule('/js/WebAudio/pcm-processor.js');
        const audioNode = new AudioWorkletNode(audio_context, 'audio-processor');
        const gainNode = audio_context.createGain();
        gainNode.gain.value = 2.0;
        audioNode.connect(gainNode);
        audioNode.connect(audio_context.destination);
        audioNode.port.onmessage = (event) => {
            if (event.data.type === "needData") {
                var bufferPtr = exports.MyPitch.Browser.Interop.WriteToSink();
                var length = blocksize * 2;

                var floatData = new Float32Array(
                    runtime.Module.HEAPF32.buffer,
                    bufferPtr,
                    length
                ).slice();

                audioNode.port.postMessage({
                    type: "audioData",
                    audioData: floatData
                });
            }
        };
    }
}

export function postAudioData(data, length) {

}

export function baseHref() {
    return globalThis.window.location.origin;
}
