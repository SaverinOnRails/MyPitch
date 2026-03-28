var synth;
var startedLoadingSynth
export function startSynth() {
    //hack for browser autoplay rules
    document.body.onclick = function () {
        if (startedLoadingSynth) return;
        startedLoadingSynth = true;
        JSSynth.waitForReady().then(loadSynth);

    }
}
async function loadSynth() {
    var context = new AudioContext();
    synth = new JSSynth.Synthesizer();
    synth.init(context.sampleRate);

    // Create AudioNode (ScriptProcessorNode) to output audio data
    var node = synth.createAudioNode(context, 8192); // 8192 is the frame count of buffer
    node.connect(context.destination);

    var sfontBuffer = await loadArrayBuffer("/SoundFonts/default.sf2");
    synth.loadSFont(sfontBuffer).then(function () {
        //  synth.midiNoteOn(0, 60, 127)
    })
}
async function loadArrayBuffer(url) {
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error(`Failed to load: ${response.status}`);
    }
    return await response.arrayBuffer();
}

export function noteOn(channel, note) {
    synth.midiNoteOn(channel, note, 127);
}
export function NoteOff(channel, note) {
    //ignore for now
}
