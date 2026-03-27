using Android.Bluetooth;
using Android.Hardware.Lights;
using Android.Icu.Util;
using Android.Media;
using MeltySynth;
using Synthesizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AndroidOS = Android.OS;
using AndroidMedia = Android.Media;
namespace MyPitch.Droid;

internal class AudioTrackDriver : IAudioDriver
{
    public static int SAMPLE_RATE = 44100;
    private MeltySynth.Synthesizer _inner;
    private AudioTrack _audioTrack;

    private float[] _interlaced;
    public AudioTrackDriver(string soundFont)
    {
        _inner = new MeltySynth.Synthesizer(soundFont, SAMPLE_RATE);
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
        Debug.WriteLine($"Min buffer size of {minBufferSize}");
        _audioTrack = new AudioTrack.Builder()
            .SetAudioAttributes(audioAttributes)
            .SetAudioFormat(audioFormat)
            .SetTransferMode(AudioTrackMode.Stream)
            .SetBufferSizeInBytes(blocksize)
            .Build();
        _interlaced = new float[blocksize * 2];
        _audioTrack.Play();
        Task.Factory.StartNew(WriteToSink, TaskCreationOptions.LongRunning);
    }
    public void Play(int note)
    {
        _inner.NoteOn(0, note, 100);
    }
    public void WriteToSink()
    {
        while (true)
        {
            _inner.RenderInterleaved(_interlaced);
            _audioTrack.Write(_interlaced, 0, _interlaced.Length, WriteMode.Blocking);
        }
    }
    public void Stop()
    {

    }
    public void Release()
    {
        _inner.NoteOffAll(false);
    }
}