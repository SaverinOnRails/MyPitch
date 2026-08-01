using Android.Media;
using MeltySynth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AndroidMedia = Android.Media;
namespace MyPitch.Droid;

internal class AudioTrackDriver : IAudioDriver
{
    public static int SAMPLE_RATE = 44100;
    private MeltySynth.Synthesizer _synth;
    private MeltySynth.Synthesizer _droneSynth;
    private float[] _temp;
    private AudioTrack _audioTrack;
    private List<string> _speechSamples = new();
    private float[] _interlaced;
    private MediaPlayer _player = new();

    private string _soundFontPath;
    private string _droneSoundFontPath;

    public AudioTrackDriver(string soundFont, string droneFont)
    {
        _soundFontPath = soundFont;
        _droneSoundFontPath = droneFont;
        _synth = new MeltySynth.Synthesizer(soundFont, SAMPLE_RATE);
        _droneSynth = new MeltySynth.Synthesizer(droneFont, SAMPLE_RATE);
        var minBufferSize = AudioTrack.GetMinBufferSize(
             SAMPLE_RATE,
             ChannelOut.Stereo,
             AndroidMedia.Encoding.PcmFloat);
        var audioAttributes = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Media)
            .SetContentType(AudioContentType.Music)
            .Build()!;

        var audioFormat = new AudioFormat.Builder()
            .SetEncoding(AndroidMedia.Encoding.PcmFloat)
            .SetSampleRate(SAMPLE_RATE)
            .SetChannelMask(ChannelOut.Stereo)
            .Build()!;
        int blocksize = 512;
        int bufferSize = Math.Max(minBufferSize, blocksize * 2 * sizeof(float));
        Debug.WriteLine($"Min buffer size of {minBufferSize}");
        _audioTrack = new AudioTrack.Builder()
            .SetAudioAttributes(audioAttributes)
            .SetAudioFormat(audioFormat)
            .SetTransferMode(AudioTrackMode.Stream)
            .SetBufferSizeInBytes(bufferSize * 2)
            .Build();
        _interlaced = new float[blocksize * 2];
        _temp = new float[blocksize * 2];
        _audioTrack.Play();
        _ = Task.Run(() =>
        {
            WriteToSink();
        });

        //Can probably convert these too PCMs and play directly with AudioTrack for more performance but honestly who cares
        var names = new string[] { "1", "2", "3", "4", "5", "6", "7", "flat-2", "flat-3", "flat-6", "flat-7", "sharp-4" };
        foreach (var name in names)
        {
            var stream = EmbeddedResources.GetSpeechSample(name);
            if (stream is null)
            {
                continue;
            }
            // Copy the stream into a byte array
            byte[] wavData;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                wavData = ms.ToArray();
            }
            var tempFile = Path.Combine(Path.GetTempPath(), $"{name}.wav");
            File.WriteAllBytes(tempFile, wavData);
            _speechSamples.Add(tempFile);
        }
    }
    public void Play(int note)
    {
        _synth.NoteOn(0, note, 127);
    }

    public void PlayChord(List<int> notes)
    {
        // Release all causes a crash in meltysynth for some reason
        // ReleaseAll();
        foreach (int note in notes)
        {
            Release(note);
            Play(note);
        }
    }

    private void WriteToSink()
    {
        while (true)
        {
            _synth.RenderInterleaved(_interlaced);
            _droneSynth.RenderInterleaved(_temp);

            //combine 
            for (int i = 0; i < _interlaced.Length; i++)
            {
                _interlaced[i] += _temp[i];
            }
            _audioTrack.Write(_interlaced, 0, _interlaced.Length, WriteMode.Blocking);
        }
    }

    public void Release(int note)
    {
        _synth.NoteOff(0, note);
    }

    public void PlayDrone(int note, int velocity)
    {
        ReleaseDrone();
        _droneSynth.NoteOn(0, note, velocity);
    }

    public void ReleaseDrone()
    {
        _droneSynth.NoteOffAll(false);
    }

    public void PlaySpeechSample(string sample)
    {
        _player.Reset();
        var path = Path.Combine(Path.GetTempPath(), $"{sample}.wav".Replace("♭", "flat-").Replace("#", "sharp-"));
        _player.SetDataSource(path);
        _player.Prepare();
        _player.Start();
    }

    public void ReleaseAll()
    {
        _synth.NoteOffAll(true);
    }
}
