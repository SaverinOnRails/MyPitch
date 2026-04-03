using MyPitch.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyPitch.Models;

internal class Game
{
    private bool _playDrone = true;
    private bool _playedCadence = false;
    private int _gameClickTimeout = 500; //ms
    private CancellationTokenSource _gameCancellationTokenSource = new();
    public IEnumerable<DegreeItem> AllowDegrees = new ObservableCollection<DegreeItem>();

    public List<string> AllowedDegreeStrings => AllowDegrees.Where(p => p.IsSelected == true).Select(p => p.Label).ToList();
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

    public async Task Start()
    {
        _gameCancellationTokenSource = new();
        try
        {
            if (_playDrone) PlayDrone();
            IsPlaying = true;
            if (Mode == GameMode.Freeplay)
            {
                //nothing to do here, let the user do whatever
            }
            else if (Mode == GameMode.Interactive)
            {
                await InteractiveGameLoop();
            }
            else //pocketmode
            {
                //nothing yet
            }
        }
        catch (Exception Ex)
        {
            if (Ex is OperationCanceledException e)
            {
                Debug.WriteLine("Game cancelled");
            }
            Stop(); //No harm in calling stop again
        }

    }
    private async Task InteractiveGameLoop()
    {
        while (true)
        {
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (!_playedCadence)
            {
                await PlayCadence();
            }
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Delay(_gameClickTimeout * 2, _gameCancellationTokenSource.Token);
            await StartQuiz();
        }
    }
    private async Task StartQuiz()
    {
        _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
        var notes = AllowedDegreeStrings;
        if (notes.Count == 0) { return; } //TODO: handle this
        var randomNote = notes[Random.Shared.Next(notes.Count)];
        await PlayScaleNote(randomNote, false);
    }

    private async Task PlayCadence()
    {
        await PlayScaleNote("1", false);
        await PlayScaleNote("4", false);
        await PlayScaleNote("5", false);
        await PlayScaleNote("1", false);
        _playedCadence = true;
    }
    private async Task PlayScaleNote(string deg, bool hidden)
    {
        _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
        var noteAtDeg = MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1, false);
        var note = MusicTheory.ToMidiNote(Tonic.ToString(), noteAtDeg);
        if (!hidden)
        {
            var fifthSegment = MusicTheory.FifthSegment(Tonic, noteAtDeg);
            GameClickedIndex = fifthSegment;
        }
        ServiceProvider.AudioDriver.Play(note);
        try
        {
            await Task.Delay(_gameClickTimeout, _gameCancellationTokenSource.Token);
        }
        finally
        {
            ServiceProvider.AudioDriver.Release(note);
        }
        if (!hidden)
        {
            GameClickedIndex = null;
        }
    }
    public void Stop()
    {
        Debug.WriteLine("Stopping game");
        _gameCancellationTokenSource.Cancel();
        SuspendDrone();
        IsPlaying = false;
        _playedCadence = false;
        GameClickedIndex = null;
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
    public async Task TogglePlay()
    {
        if (IsPlaying)
            Stop();
        else
            await Start();
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

