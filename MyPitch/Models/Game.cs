using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyPitch.Models;

public partial class Game : ObservableObject
{
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private int? _gameClickedIndex;
    [ObservableProperty] private AnswerState _answerState;
    [ObservableProperty] private Key _tonic = Key.C;
    [ObservableProperty] private int _octave = 4;

    private readonly bool _playDrone = true;
    private bool _playedCadence;
    private const int GameClickTimeout = 500; // ms
    private CancellationTokenSource _cts = new();
    private TaskCompletionSource<int>? _userClickTcs;


    public GameSettings Settings { get; private set; } = new();

    public void ApplySettings(GameSettings settings)
    {
        Settings = settings;
    }

    public IEnumerable<DegreeItem> AllowDegrees { get; set; } = new ObservableCollection<DegreeItem>();

    private List<string> AllowedDegreeStrings =>
        AllowDegrees.Where(d => d.IsSelected).Select(d => d.Label).ToList();


    private int? _userClickedIndex;
    public int? UserClickedIndex
    {
        get => _userClickedIndex;
        set
        {
            _userClickedIndex = value;
            if (value is not null)
                _userClickTcs?.TrySetResult(value.Value);
        }
    }


    partial void OnTonicChanged(Key value)
    {
        if (_playDrone && IsPlaying)
        {
            SuspendDrone();
            PlayDrone();
        }
    }

    public async Task TogglePlay()
    {
        if (IsPlaying) Stop();
        else await Start();
    }

    public void Stop()
    {
        _cts.Cancel();
        SuspendDrone();
        IsPlaying = false;
        AnswerState = AnswerState.Neutral;
        _playedCadence = false;
        GameClickedIndex = null;
    }



    private async Task Start()
    {
        _cts = new CancellationTokenSource();
        try
        {
            if (_playDrone) PlayDrone();
            IsPlaying = true;

            await (Settings.Mode switch
            {
                GameMode.Interactive => InteractiveGameLoop(),
                GameMode.Freelisten => FreeListenGameLoop(),
                GameMode.Pocketmode => PocketModeGameLoop(),
                _ => Task.CompletedTask   // Freeplay 
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            // throw ex;
            Stop();
        }
    }

    private async Task PocketModeGameLoop()
    {
        while (true)
        {
            _cts.Token.ThrowIfCancellationRequested();

            await MaybeChangeTonic();
            await MaybePlayCadence();

            _cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _cts.Token);

            var quizDeg = await PlayQuizNote(hidden: true);
            await Task.Delay(1000, _cts.Token);
            PlatformServiceProvider.AudioDriver.PlaySpeechSample(quizDeg); //exception here
            await Task.Delay(1000, _cts.Token);
        }
    }

    private async Task InteractiveGameLoop()
    {
        while (true)
        {
            AnswerState = AnswerState.Neutral;
            _cts.Token.ThrowIfCancellationRequested();

            await MaybeChangeTonic();
            await MaybePlayCadence();

            _cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _cts.Token);

            var quizDeg = await PlayQuizNote(hidden: true);
            var quizNoteIndex = MusicTheory.FifthSegment(
                Tonic,
                MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(quizDeg) + 1, false));

            _userClickTcs = new TaskCompletionSource<int>();
            var userResponse = await _userClickTcs.Task;

            if (userResponse == quizNoteIndex)
            {
                AnswerState = AnswerState.Correct;
                GameClickedIndex = quizNoteIndex;
                await Task.Delay(1000, _cts.Token);
                GameClickedIndex = null;
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                for (var i = 0; i < 10; i++)
                {
                    GameClickedIndex = quizNoteIndex;
                    await Task.Delay(200, _cts.Token);
                    GameClickedIndex = null;
                    await Task.Delay(50, _cts.Token);
                }
            }
        }
    }

    private async Task FreeListenGameLoop()
    {
        while (true)
        {
            _cts.Token.ThrowIfCancellationRequested();

            await MaybeChangeTonic();
            await MaybePlayCadence();

            _cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _cts.Token);

            await PlayQuizNote(hidden: false);
        }
    }

    private async Task MaybeChangeTonic()
    {
        var oldTonic = Tonic;

        if (Settings.RandomTonic)
            Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];

        if (Settings.RandomOctave)
        {
            int[] octaveRange = [3, 4, 5];
            Octave = octaveRange[Random.Shared.Next(octaveRange.Length)];
        }
        if (Settings.PlayCadenceOnKeyChange && oldTonic != Tonic)
            _playedCadence = false;
    }

    private async Task MaybePlayCadence()
    {
        if (!_playedCadence)
            await PlayCadence();
    }

    private async Task<string> PlayQuizNote(bool hidden)
    {
        _cts.Token.ThrowIfCancellationRequested();

        var degrees = AllowedDegreeStrings;
        if (degrees.Count == 0) return "";

        var randomNote = degrees[Random.Shared.Next(degrees.Count)];
        await PlayScaleNote(randomNote, hidden);
        return randomNote;
    }

    private async Task PlayCadence()
    {
        foreach (var deg in new[] { "1", "4", "5", "1" })
            await PlayScaleNote(deg, hidden: false);

        _playedCadence = true;
    }

    private async Task PlayScaleNote(string deg, bool hidden)
    {
        _cts.Token.ThrowIfCancellationRequested();

        var noteAtDeg = MusicTheory.NoteAtDegree(
            Tonic,
            MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1,
            false);

        var note = MusicTheory.ToMidiNote(Tonic.ToString(), noteAtDeg, Octave);

        if (!hidden)
            GameClickedIndex = MusicTheory.FifthSegment(Tonic, noteAtDeg);

        PlatformServiceProvider.AudioDriver.Play(note);
        try
        {
            await Task.Delay(GameClickTimeout, _cts.Token);
        }
        finally
        {
            PlatformServiceProvider.AudioDriver.Release(note);
            if (!hidden) GameClickedIndex = null;
        }
    }

    private void PlayDrone()
    {
        var note = MusicTheory.ToMidiNote(Tonic.ToString(), Tonic.ToString());
        PlatformServiceProvider.AudioDriver.PlayDrone(note);
    }

    private void SuspendDrone() => PlatformServiceProvider.AudioDriver.ReleaseDrone();
}

public record GameSettings(
    GameMode Mode = GameMode.Freeplay,
    bool RandomTonic = false,
    bool RandomOctave = false,
    bool PlayCadenceOnKeyChange = true
);

public enum GameMode { Freeplay, Pocketmode, Interactive, Freelisten }
public enum AnswerState { Correct, Neutral, Incorrect }