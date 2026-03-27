namespace MyPitch.Desktop;

using System;
using System.IO;
using System.Runtime.InteropServices;

internal static class LibFluidSynth
{
    private const string LibraryName = "fluidsynth";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint new_fluid_settings();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void delete_fluid_settings(nint settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int fluid_settings_setstr(nint settings, string name, string str);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int fluid_settings_setnum(nint settings, string name, double val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int fluid_settings_setint(nint settings, string name, int val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint new_fluid_synth(nint settings);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void delete_fluid_synth(nint synth);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int fluid_synth_sfload(nint synth, string filename, int reset_presets);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_sfunload(nint synth, int sfont_id, int reset_presets);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_noteon(nint synth, int chan, int key, int vel);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_noteoff(nint synth, int chan, int key);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_program_change(nint synth, int chan, int program);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_bank_select(nint synth, int chan, int bank);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_cc(nint synth, int chan, int ctrl, int val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_pitch_bend(nint synth, int chan, int val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_channel_pressure(nint synth, int chan, int val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_key_pressure(nint synth, int chan, int key, int val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint new_fluid_audio_driver(nint settings, nint synth);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void delete_fluid_audio_driver(nint driver);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_system_reset(nint synth);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_all_notes_off(nint synth, int chan);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fluid_synth_all_sounds_off(nint synth, int chan);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fluid_synth_set_gain(nint synth, float gain);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float fluid_synth_get_gain(nint synth);
}

public class FluidSynth : IDisposable
{
    private nint _settings;
    private nint _synth;
    private nint _audioDriver;
    private bool _disposed;

    public nint Synth => _synth;

    public FluidSynth(string soundFont)
    {
        _settings = LibFluidSynth.new_fluid_settings();
        if (_settings == nint.Zero)
            throw new Exception("Failed to create LibFluidSynthsettings");
        LibFluidSynth.fluid_settings_setstr(_settings, "audio.driver", "pipewire");
        string driver =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dsound" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "pulseaudio" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "coreaudio" :
            throw new PlatformNotSupportedException();

        LibFluidSynth.fluid_settings_setstr(_settings, "audio.driver", driver);

        // LibFluidSynth.fluid_settings_setint(_settings, "synth.reverb.active", 1);
        // LibFluidSynth.fluid_settings_setint(_settings, "synth.chorus.active", 1);
        LibFluidSynth.fluid_settings_setint(_settings, "audio.period-size", 1024);
        LibFluidSynth.fluid_settings_setint(_settings, "audio.periods", 8);
        LibFluidSynth.fluid_settings_setint(_settings, "audio.sample-rate", 44100);

        LibFluidSynth.fluid_settings_setint(_settings, "synth.polyphony", 128);
        LibFluidSynth.fluid_settings_setint(_settings, "synth.reverb.active", 0);
        LibFluidSynth.fluid_settings_setint(_settings, "synth.chorus.active", 0);
        LibFluidSynth.fluid_settings_setint(_settings, "audio.realtime-prio", 0);

        _synth = LibFluidSynth.new_fluid_synth(_settings);
        if (_synth == nint.Zero)
            throw new Exception("Failed to create LibFluidSynthsynthesizer");

        _audioDriver = LibFluidSynth.new_fluid_audio_driver(_settings, _synth);
        if (_audioDriver == nint.Zero)
            throw new Exception("Failed to create LibFluidSynthaudio driver");
        LoadSoundFont(soundFont);
        SetGain(2f);
 //       ControlChange(0, 64, 127);
    }

    public int LoadSoundFont(string path)
    {
        return LibFluidSynth.fluid_synth_sfload(_synth, path, 1);
    }

    public void NoteOn(int channel, int note, int velocity)
    {
        LibFluidSynth.fluid_synth_noteon(_synth, channel, note, velocity);
    }

    public void NoteOff(int channel, int note)
    {
        LibFluidSynth.fluid_synth_noteoff(_synth, channel, note);
    }

    public void ProgramChange(int channel, int program)
    {
        LibFluidSynth.fluid_synth_program_change(_synth, channel, program);
    }

    public void ControlChange(int channel, int controller, int value)
    {
        LibFluidSynth.fluid_synth_cc(_synth, channel, controller, value);
    }

    public void PitchBend(int channel, int value)
    {
        LibFluidSynth.fluid_synth_pitch_bend(_synth, channel, value);
    }

    public void AllNotesOff(int channel)
    {
        LibFluidSynth.fluid_synth_all_notes_off(_synth, channel);
    }

    public void SetGain(float gain)
    {
        LibFluidSynth.fluid_synth_set_gain(_synth, gain);
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_audioDriver != nint.Zero)
            LibFluidSynth.delete_fluid_audio_driver(_audioDriver);

        if (_synth != nint.Zero)
            LibFluidSynth.delete_fluid_synth(_synth);

        if (_settings != nint.Zero)
            LibFluidSynth.delete_fluid_settings(_settings);

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~FluidSynth()
    {
        Dispose();
    }
}