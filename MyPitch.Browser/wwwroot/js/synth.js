var synth;
var startedLoadingSynth;
var context;
var synthReady = false;
export function startSynth() {
    JSSynth.waitForReady().then(loadSynth);
    document.body.onclick = function () {
        if (context.state == 'suspended') {
            console.log("suspended, resuming");
            context.resume();
        }
    }
}

async function loadSynth() {
    context = new AudioContext();
    synth = new JSSynth.Synthesizer();
    synth.init(context.sampleRate);
    // Create AudioNode (ScriptProcessorNode) to output audio data
    var node = synth.createAudioNode(context, 8192); // 8192 is the frame count of buffer
    node.connect(context.destination);

    var sfontBuffer1 = await loadArrayBuffer("/Soundfonts/default.sf2");
    var sfontBuffer2 = await loadArrayBuffer("/Soundfonts/warm pad.sf2");

    synth.loadSFont(sfontBuffer1).then(function (sf1) {
        //  synth.midiNoteOn(0, 60, 127)
        synth.loadSFont(sfontBuffer2).then(function (sf2) {
            synth.midiProgramSelect(0, sf1, 0, 0);
            synth.midiProgramSelect(5, sf2, 0, 0);
            synthReady = true;
        })
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
export function noteOff(channel, note) {
    if (!synthReady) { synthNotReady(); return; }
    synth.midiNoteOff(channel, note);
}
export function allNotesOff(channel) {
    if (!synthReady) { synthNotReady(); return; }
    synth.midiAllNotesOff(channel);
}
function synthNotReady() {
    alert("Browser Synth is not ready yet, Please try again.");
}
export async function playSpeechSample(sample) {
    const audioBuffer = await context.decodeAudioData(new Uint8Array(sample).buffer);
    const source = context.createBufferSource();
    source.buffer = audioBuffer;
    source.connect(context.destination);
    source.start();

}