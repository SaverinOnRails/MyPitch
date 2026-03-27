using Synthesizer;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
namespace MyPitch.Desktop;

public class FluidAudioDriver : IAudioDriver, IDisposable
{
    private bool disposedValue;
    private FluidSynth _synth;
    public FluidAudioDriver(string soundFont)
    {
        _synth = new(soundFont);
    }
    public void Play(int note)
    {
        _synth.NoteOn(0, note, 127);
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~OpenAlAudioDriver()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Release()
    {
        _synth.AllNotesOff(0);
    }
}