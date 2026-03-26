
using Silk.NET.OpenAL;
using Synthesizer;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MyPitch.Desktop;

public unsafe class OpenAlAudioDriver : IAudioDriver, IDisposable
{
    public const int SAMPLE_RATE = 44100;
    private const int BUFFER_COUNT = 3; 
    private const int BUFFER_SIZE_SAMPLES = 1024; 

    private MeltySynth.Synthesizer _inner;
    private bool _disposedValue;

    private ALContext _alc;
    private AL _al;
    private Device* _alDevice;
    private Context* _alContext;

    private uint _source;
    private uint[] _buffers;

    private float[] _left;
    private float[] _right;

    private CancellationTokenSource _cts;
    private Task _streamTask;

    public OpenAlAudioDriver(string soundFontLocation)
    {
        _inner = new MeltySynth.Synthesizer(soundFontLocation, SAMPLE_RATE);

        _alc = ALContext.GetApi(true);
        _al = AL.GetApi();
        _alDevice = _alc.OpenDevice("");
        _alContext = _alc.CreateContext(_alDevice, null);
        _alc.MakeContextCurrent(_alContext);

        _left = new float[BUFFER_SIZE_SAMPLES];
        _right = new float[BUFFER_SIZE_SAMPLES];

        _source = _al.GenSource();
        _buffers = _al.GenBuffers(BUFFER_COUNT);

        _cts = new CancellationTokenSource();
        _streamTask = Task.Run(() => StreamLoop(_cts.Token));
    }

    public void Play()
    {
        _inner.NoteOn(0, 60, 100);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _al.SourceStop(_source);
    }

    private void StreamLoop(CancellationToken token)
    {
        foreach (var bufferId in _buffers)
        {
            FillAndQueueBuffer(bufferId);
        }

        _al.SourcePlay(_source);

        // 2. Continuous Polling
        while (!token.IsCancellationRequested)
        {
            _al.GetSourceProperty(_source, GetSourceInteger.BuffersProcessed, out int processedCount);

            while (processedCount > 0)
            {
                uint bufferId = 0;
                _al.SourceUnqueueBuffers(_source, 1, &bufferId);

                FillAndQueueBuffer(bufferId);

                processedCount--;

                _al.GetSourceProperty(_source, GetSourceInteger.SourceState, out int state);
                if ((SourceState)state != SourceState.Playing)
                {
                    _al.SourcePlay(_source);
                }
            }
            Thread.Sleep(5);
        }
    }

    private void FillAndQueueBuffer(uint bufferId)
    {
        _inner.Render(_left, _right);

        // Convert and Interlace
        var pcm = Helper.InterlacePcm(_left, _right);

        fixed (short* pPcm = pcm)
        {
            _al.BufferData(bufferId, BufferFormat.Stereo16, pPcm, pcm.Length * sizeof(short), SAMPLE_RATE);
        }

        _al.SourceQueueBuffers(_source, 1, &bufferId);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        _cts?.Cancel();
        _streamTask?.Wait();

        if (disposing)
        {
            _al.Dispose();
            _alc.Dispose();
        }

        if (_source != 0)
        {
            _al.SourceStop(_source);
            _al.DeleteSource(_source);
        }

        if (_buffers != null)
        {
            fixed (uint* pBuffers = _buffers)
                _al.DeleteBuffers(BUFFER_COUNT, pBuffers);
        }

        if (_alContext != null)
        {
            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_alContext);
        }

        if (_alDevice != null)
        {
            _alc.CloseDevice(_alDevice);
        }

        _disposedValue = true;
    }

    ~OpenAlAudioDriver() => Dispose(false);
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}