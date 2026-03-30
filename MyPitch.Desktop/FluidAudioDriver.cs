using System;
using System.Diagnostics;
using System.IO;
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
        _synth = new(soundFont, Path.Join(AppContext.BaseDirectory, "warm pad.sf2"));
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
            _synth.Dispose();
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

    public void Release(int note)
    {
        _synth.NoteOff(0, note);
    }

    public void PlayDrone(int note)
    {
        ReleaseDrone();
        _synth.NoteOn(5, note, 100);
    }

    public void ReleaseDrone()
    {
        _synth.AllNotesOff(5);
    }
}