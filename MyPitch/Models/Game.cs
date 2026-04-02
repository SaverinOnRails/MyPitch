using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyPitch.Models;

internal class Game
{
    private bool _playDrone = true;
    private bool _playedCadence = false;
    private int _gameClickTimeout = 500; //ms
    public bool IsPlaying
    {
        get;
        private set
        {
            field = value;
            RaisePlayingStatusChanged();
        }
    }
    public int? GameClickedIndex
    {
        get;
        private set
        {
            field = value;
            RaiseGameClickedIndexChanged();
        }
    }

    public Key Tonic
    {
        get; set
        {
            field = value;
            //Restart drone if playing
            if (_playDrone && IsPlaying)
            {
                SuspendDrone();
                PlayDrone();
            }
        }
    }
    public GameMode Mode { get; set; }

    public void Start()
    {
        if (_playDrone) PlayDrone();
        IsPlaying = true;
        if (Mode == GameMode.Freeplay)
        {
            //nothing to do here, let the user do whatever
        }
        else if (Mode == GameMode.Interactive)
        {
            InteractiveGameLoop();
        }
        else //pocketmode
        {
            //nothing yet
        }

    }
    private void InteractiveGameLoop()
    {
        if (!_playedCadence)
        {
            PlayCadence();
        }
    }

    private async void PlayCadence()
    {
        await PlayScaleNote("1", false);
        await PlayScaleNote("4", false);
        await PlayScaleNote("5", false);
        await PlayScaleNote("1", false);

    }
    private async Task PlayScaleNote(string deg, bool hidden)
    {
        var noteAtDeg = MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1, false);
        var note = MusicTheory.ToMidiNote(Tonic.ToString(), noteAtDeg);
        if (!hidden)
        {
            var fifthSegment = MusicTheory.FifthSegment(Tonic, noteAtDeg);
            Debug.WriteLine($"Segment for {noteAtDeg} is {fifthSegment} ");
            GameClickedIndex = fifthSegment;
        }
        ServiceProvider.AudioDriver.Play(note);
        await Task.Delay(_gameClickTimeout);
        ServiceProvider.AudioDriver.Release(note);
        if (!hidden)
        {
            GameClickedIndex = null;
        }
    }
    public void Stop()
    {
        SuspendDrone();
        IsPlaying = false;
        _playedCadence = false;
    }

    private void PlayDrone()
    {
        var note = MusicTheory.ToMidiNote(Tonic.ToString(), Tonic.ToString()); //wtf honestly
        ServiceProvider.AudioDriver.PlayDrone(note);
    }
    private void SuspendDrone()
    {
        ServiceProvider.AudioDriver.ReleaseDrone();
    }
    public void TogglePlay()
    {
        if (IsPlaying)
            Stop();
        else
            Start();
    }

    private void RaisePlayingStatusChanged()
    {
        PlayingStatusChanged?.Invoke(this, new());
    }
    private void RaiseGameClickedIndexChanged()
    {
        GameClickedIndexChanged?.Invoke(this, new());
    }

    public event EventHandler? PlayingStatusChanged;
    public event EventHandler? GameClickedIndexChanged;
}

public enum GameMode
{
    Freeplay,
    Pocketmode,
    Interactive
}

