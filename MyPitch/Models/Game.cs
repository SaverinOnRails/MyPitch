using MyPitch.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyPitch.Models;

internal class Game
{
    private bool _playDrone = true;
    private bool _playedCadence = false;
    public bool RandomTonic = false;
    public bool PlayCadenceOnKeyChange = false;
    private Key _oldTonic;
    private int _gameClickTimeout = 500; //ms
    private CancellationTokenSource _gameCancellationTokenSource = new();
    public IEnumerable<DegreeItem> AllowDegrees = new ObservableCollection<DegreeItem>();
    private TaskCompletionSource<int>? _userClickTcs;
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
    public AnswerState AnswerState
    {
        get;
        private set
        {
            field = value;
            RaiseAnswerStateChanged();
        }
    }
    public int? UserClickedIndex
    {
        get;
        set
        {
            field = value;
            if (value is not null)
            {
                _userClickTcs?.TrySetResult(value.Value);
            }
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
            RaiseTonicChanged();
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
            else if (Mode == GameMode.Freelisten)
            {
                await FreeListenGameLoop();
            }
            else //pocketmode
            {
                //nothing yet
            }
        }
        catch
        {
            //if (Ex is OperationCanceledException e)
            //{
            //    Debug.WriteLine("Game cancelled");
            //}
            Stop(); //No harm in calling stop again
        }

    }

    private async Task InteractiveGameLoop()
    {
        while (true)
        {
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            _oldTonic = Tonic;
            if (RandomTonic)
            {
                Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
            }
            if (!_playedCadence || (PlayCadenceOnKeyChange && _oldTonic != Tonic))
            {
                await PlayCadence();
            }
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Delay(_gameClickTimeout * 2, _gameCancellationTokenSource.Token);
            AnswerState = AnswerState.Neutral;
            var quizDeg = await PlayQuizNote(true);
            var quizNoteIndex = MusicTheory.FifthSegment(Tonic, MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(quizDeg) + 1, false));
            //await user response
            _userClickTcs = new TaskCompletionSource<int>();
            var userResponse = await _userClickTcs.Task;
            if (userResponse == quizNoteIndex)
            {
                AnswerState = AnswerState.Correct;
                GameClickedIndex = quizNoteIndex;
                await Task.Delay(1000, _gameCancellationTokenSource.Token);
                GameClickedIndex = null;
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                for (var i = 0; i < 10; i++)
                {
                    GameClickedIndex = quizNoteIndex;
                    await Task.Delay(200, _gameCancellationTokenSource.Token);
                    GameClickedIndex = null;
                    await Task.Delay(50, _gameCancellationTokenSource.Token);

                }
            }
        }
    }

    private async Task FreeListenGameLoop()
    {
        while (true)
        {
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            if (RandomTonic)
            {
                Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
            }
            if (!_playedCadence)
            {
                await PlayCadence();
            }
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Delay(_gameClickTimeout * 2, _gameCancellationTokenSource.Token);
            await PlayQuizNote(false);
        }
    }
    private async Task<string> PlayQuizNote(bool hidden)
    {
        _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
        var degrees = AllowedDegreeStrings;
        if (degrees.Count == 0) { return ""; } //TODO: handle this
        var randomNote = degrees[Random.Shared.Next(degrees.Count)];
        await PlayScaleNote(randomNote, hidden);
        return randomNote;
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
        _gameCancellationTokenSource.Cancel();
        SuspendDrone();
        IsPlaying = false;
        AnswerState = AnswerState.Neutral;
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
    private void RaiseAnswerStateChanged()
    {
        AnswerStateChanged?.Invoke(this, new());
    }
    public void RaiseTonicChanged()
    {
        TonicChanged?.Invoke(this, new());
    }
    public event EventHandler? PlayingStatusChanged;
    public event EventHandler? GameClickedIndexChanged;
    public event EventHandler? AnswerStateChanged;
    public event EventHandler? TonicChanged;
}

public enum GameMode
{
    Freeplay,
    Pocketmode,
    Interactive,
    Freelisten
}

public enum AnswerState
{
    Correct,
    Neutral,
    Incorrect
}