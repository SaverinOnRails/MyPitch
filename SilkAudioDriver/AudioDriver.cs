using MeltySynth;
using Microsoft.VisualBasic;
using Silk.NET.OpenAL;
using Synthesizer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
namespace SilkAudioDriver;

public unsafe class OpenAlAudioDriver : IAudioDriver, IDisposable
{
    public static int SAMPLE_RATE = 44100;
    private MeltySynth.Synthesizer _inner;
    private bool _disposedValue;
    private ALContext _alc;
    private AL _al;
    private Device* _alDevice;
    private Context* _alContext;
    private uint _source;
    private uint _buffer;
    private float[] _left;
    private float[] _right;
    private string _soundFont => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "default.sf2");

    public OpenAlAudioDriver()
    {
        _inner = new MeltySynth.Synthesizer(_soundFont, SAMPLE_RATE);
        _alc = ALContext.GetApi(true);
        _al = AL.GetApi();
        _alDevice = _alc.OpenDevice("");
        _alContext = _alc.CreateContext(_alDevice, null);
        _alc.MakeContextCurrent(_alContext);
        _al.GetError();
        _left = new float[SAMPLE_RATE * 3];
        _right = new float[SAMPLE_RATE * 3];
        _source = _al.GenSource();
        _buffer = _al.GenBuffer();
    }

    public void Start() => throw new NotImplementedException();
    public void Stop() => throw new NotImplementedException();

    public void Play()
    {
        _inner.NoteOn(0, 60, 100);
        _inner.Render(_left, _right);
        SubmitSamples(_left, _right);
    }

    private void SubmitSamples(float[] left, float[] right)
    {
        var pcm = new short[left.Length * 2];
        for (int i = 0; i < left.Length; i++)
        {
            pcm[i * 2] = (short)(Math.Clamp(left[i], -1f, 1f) * short.MaxValue);
            pcm[i * 2 + 1] = (short)(Math.Clamp(right[i], -1f, 1f) * short.MaxValue);
        }

        fixed (short* pPcm = pcm)
        {
            _al.BufferData(_buffer, BufferFormat.Stereo16, pPcm, pcm.Length * sizeof(short), SAMPLE_RATE);
        }
        _al.SetSourceProperty(_source, SourceInteger.Buffer, _buffer);
        _al.SourcePlay(_source);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing)
        {
            // Managed resources: anything that is itself IDisposable
            _al.Dispose();
            _alc.Dispose();
        }
        if (_source != 0)
        {
            _al.SourceStop(_source);
            _al.DeleteSource(_source);
            _source = 0;
        }

        if (_buffer != 0)
        {
            _al.DeleteBuffer(_buffer);
            _buffer = 0;
        }
        if (_alContext != null)
        {
            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_alContext);
            _alContext = null;
        }

        if (_alDevice != null)
        {
            _alc.CloseDevice(_alDevice);
            _alDevice = null;
        }

        _disposedValue = true;
    }

    ~OpenAlAudioDriver()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

