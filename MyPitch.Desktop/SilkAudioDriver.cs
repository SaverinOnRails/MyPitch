
using Silk.NET.OpenAL;
using Synthesizer;
using System;

namespace MyPitch.Desktop;

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

    public OpenAlAudioDriver(string soundFontLocation)
    {
        _inner = new MeltySynth.Synthesizer(soundFontLocation, SAMPLE_RATE);
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

    public void Stop() => throw new NotImplementedException();

    public void Play()
    {
        _inner.NoteOn(0, 60, 100);
        _inner.Render(_left, _right);
        SubmitSamples(_left, _right);
    }

    private void SubmitSamples(float[] left, float[] right)
    {
        var pcm = Helper.InterlacePcm(left, right);
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

