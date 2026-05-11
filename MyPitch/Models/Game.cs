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
    [ObservableProperty] private int _droneVolume = 100;
    [ObservableProperty] private int _interactiveModeRounds = 0;
    [ObservableProperty] private MelodyBarState _melodyBarState = new(); // We can just change this reference to alert the view instead of implementing some change notifiers for its properties

    private const int _interactiveModeMaxRoundCount = 5;
    private bool _playedCadence;
    private const int GameClickTimeout = 500; // ms
    private CancellationTokenSource _cts = new();
    private TaskCompletionSource<int>? _userClickTcs;
    private Models.Key _oldTonic;
    public GameSettings Settings
    {
        get;
        private set;
    } = new();
    public void ApplySettings(GameSettings settings)
    {
        Settings = settings;
        if (Settings.PlayDrone && _dronePlaying == false && IsPlaying)
        {
            PlayDrone();
        }
        if (!Settings.PlayDrone)
        {
            SuspendDrone();
        }
    }
    public IEnumerable<DegreeItem> AllowDegrees
    {
        get;
        set;
    } = new ObservableCollection<DegreeItem>();
    private List<string> AllowedDegreeStrings => AllowDegrees.Where(d => d.IsSelected).Select(d => d.Label).ToList();
    private int? _userClickedIndex;
    private bool _dronePlaying;
    public int? UserClickedIndex
    {
        get => _userClickedIndex;
        set
        {
            _userClickedIndex = value;
            if (value is not null) _userClickTcs?.TrySetResult(value.Value);
        }
    }
    partial void OnTonicChanged(Key value)
    {
        if (Settings.PlayDrone && IsPlaying)
        {
            SuspendDrone();
            PlayDrone();
        }
    }
    partial void OnDroneVolumeChanged(int value)
    {
        if (_dronePlaying)
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
        InteractiveModeRounds = 0;
        MelodyBarState = new();
        _playedCadence = false;
        GameClickedIndex = null;
    }
    private async Task Start()
    {
        _cts = new CancellationTokenSource();
        try
        {
            if (Settings.PlayDrone) PlayDrone();
            IsPlaying = true;
            await (Settings.Mode
                switch
            {
                GameMode.Interactive => InteractiveGameLoop(),
                GameMode.Freelisten => FreeListenGameLoop(),
                GameMode.Pocketmode => PocketModeGameLoop(),
                GameMode.Cycle => CycleModeGameLoop(),
                GameMode.Melody => MelodyGameModeLoop(),
                _ => Task.CompletedTask // Freeplay
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            // throw ex;
            Stop();
        }
    }
    private async Task MelodyGameModeLoop()
    {
        while (true)
        {
            AnswerState = AnswerState.Neutral;
            MelodyBarState = new();
            _cts.Token.ThrowIfCancellationRequested();
            await MaybeChangeTonic();
            await MaybePlayCadence();
            await Task.Delay(GameClickTimeout * 2, _cts.Token);
            var degrees = AllowedDegreeStrings;
            var melodyNoteCount = Settings.MelodyNoteCount;
            if (degrees.Count == 0) return;
            var melody = MusicTheory.GenMelody(degrees, melodyNoteCount);
            foreach (var note in melody)
            {
                await PlayScaleNote(note, hidden: true, 1000);
            }
            //await user responses
            List<string> userResponses = new();
            for (int i = 0; i < melodyNoteCount; i++)
            {
                _userClickTcs = new TaskCompletionSource<int>();
                var userResponse = await _userClickTcs.Task.WaitAsync(_cts.Token);
                var dict = MelodyBarState.UserChoices;
                var deg = MusicTheory.FifthIntervalScaleGraduation[userResponse];
                dict[i] = deg;
                userResponses.Add(deg);
                MelodyBarState = new(dict, new());
            }
            var correct = true;
            List<int> incorrectDegs = new();
            for (int i = 0; i < melodyNoteCount; i++)
            {
                if (userResponses[i] != melody[i])
                {
                    correct = false;
                    incorrectDegs.Add(i);
                }
            }
            if (correct)
            {
                AnswerState = AnswerState.Correct;
                await Task.Delay(1000, _cts.Token);
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                MelodyBarState = new(MelodyBarState.UserChoices, incorrectDegs);
                await Task.Delay(300, _cts.Token);
                for (var i = 0; i < melodyNoteCount; i++)
                {
                    var dict = MelodyBarState.UserChoices;
                    dict[i] = melody[i];
                    MelodyBarState = new(dict, incorrectDegs);
                    await PlayScaleNote(melody[i], hidden: false, duration: 1000);
                    await Task.Delay(500, _cts.Token);
                }
            }
        }
    }
    private async Task CycleModeGameLoop()
    {
        int cycleIndex = 0;
        while (true)
        {
            int length = MusicTheory.ChromaticScaleGraduation.Length;
            _cts.Token.ThrowIfCancellationRequested();
            await MaybeChangeTonic();
            await MaybePlayCadence();
            while (!AllowedDegreeStrings.Contains(MusicTheory.ChromaticScaleGraduation[cycleIndex]))
            {
                if (AllowedDegreeStrings.Count() == 0) break;
                _cts.Token.ThrowIfCancellationRequested();
                cycleIndex = (cycleIndex + 1) % length;
            }
            string degAtCycleIndex = MusicTheory.ChromaticScaleGraduation[cycleIndex];
            await PlayScaleNote(degAtCycleIndex, hidden: false, duration: 2000);
            cycleIndex = (cycleIndex + 1) % length;
            await Task.Delay(200, _cts.Token);
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
            PlatformServiceProvider.AudioDriver.PlaySpeechSample(quizDeg);
            await Task.Delay(1000, _cts.Token);
        }
    }
    private async Task InteractiveGameLoop()
    {
        var stats = new InteractiveModeStats();
        TimeSpan totalResponseTime = TimeSpan.Zero;
        while (true)
        {
            var now = DateTime.Now;
            AnswerState = AnswerState.Neutral;
            var tonic = Tonic;
            _cts.Token.ThrowIfCancellationRequested();
            await MaybeChangeTonic();
            await MaybePlayCadence();
            _cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _cts.Token);
            var quizDeg = await PlayQuizNote(hidden: true);
            var quizNoteIndex = MusicTheory.FifthSegment(tonic, MusicTheory.NoteAtDegree(tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(quizDeg) + 1, false));
            _userClickTcs = new TaskCompletionSource<int>();
            var responseStart = DateTime.Now;
            var userResponse = await _userClickTcs.Task.WaitAsync(_cts.Token);
            var responseEnd = DateTime.Now - responseStart;
            totalResponseTime += responseEnd;
            if (userResponse == quizNoteIndex)
            {
                AnswerState = AnswerState.Correct;
                GameClickedIndex = quizNoteIndex;
                await Task.Delay(1000, _cts.Token);
                if (MusicTheory.SimpleResolutionMap.ContainsKey(quizDeg))
                {
                    var resolution = MusicTheory.SimpleResolutionMap[quizDeg];
                    var resolutionIndex = MusicTheory.FifthSegment(tonic, MusicTheory.NoteAtDegree(tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(resolution) + 1, false));
                    await PlayScaleNote(resolution, hidden: false, duration: 500);
                }
                stats.AddStatForDeg(quizDeg, responseEnd, true, null);
                GameClickedIndex = null;
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                await Task.Delay(400, _cts.Token);
                for (var i = 0; i < 5; i++)
                {
                    await PlayScaleNote(quizDeg, hidden: false, duration: 200);
                    await Task.Delay(50, _cts.Token);
                }
                stats.AddStatForDeg(quizDeg, responseEnd, false, MusicTheory.FifthIntervalScaleGraduation[userResponse]);
            }

            if (InteractiveModeRounds == _interactiveModeMaxRoundCount)
            {
                Debug.WriteLine("Mode complete");
                stats.AverageResponseTime = ((totalResponseTime) / InteractiveModeRounds);
                // Debug.WriteLine($"Average Response Time {stats.AverageResponseTime.TotalSeconds}");

                // foreach (var degStat in stats.DegreeStats)
                // {
                //     Debug.WriteLine($"deg : {degStat.Key}");
                //     var stat = degStat.Value;
                //     Debug.WriteLine($"Times correct {stat.TimesCorrect}");
                //     Debug.WriteLine($"Times incorrect {stat.TimesIncorrect}");
                //     Debug.WriteLine($"Average Response Time{stat.AverageResponseTime.TotalSeconds}");
                //     Debug.WriteLine($"Mistaken For :");
                //     foreach (var p in stat.MistakenFor) Debug.WriteLine(p);
                //     Debug.WriteLine("");
                // }
                _cts.Cancel();
            }
            InteractiveModeRounds++;
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
            await Task.Delay(1000, _cts.Token);
        }
    }
    private async Task MaybeChangeTonic()
    {
        if (Settings.RandomTonic) Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
        if (Settings.RandomOctave)
        {
            int[] octaveRange = [3, 4, 5];
            Octave = octaveRange[Random.Shared.Next(octaveRange.Length)];
        }
        if (Settings.PlayCadenceOnKeyChange && _oldTonic != Tonic)
        {
            _playedCadence = false;
        }
        _oldTonic = Tonic;
    }
    private async Task MaybePlayCadence()
    {
        if (!_playedCadence) await PlayCadence();
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
        foreach (var deg in new[]
        {
            "1",
            "4",
            "5",
            "1"
        })
            await PlayScaleNote(deg, hidden: false);
        _playedCadence = true;
    }
    private async Task PlayScaleNote(string deg, bool hidden, int duration = 500)
    {
        _cts.Token.ThrowIfCancellationRequested();
        var noteAtDeg = MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1, false);
        var note = MusicTheory.ToMidiNote(Tonic, noteAtDeg, Octave);
        if (!hidden) GameClickedIndex = MusicTheory.FifthSegment(Tonic, noteAtDeg);
        PlatformServiceProvider.AudioDriver.Play(note);
        try
        {
            await Task.Delay(duration, _cts.Token);
        }
        finally
        {
            PlatformServiceProvider.AudioDriver.Release(note);
            if (!hidden) GameClickedIndex = null;
        }
    }
    private void PlayDrone()
    {
        _dronePlaying = true;
        var note = MusicTheory.ToMidiNote(Tonic, Tonic.ToString());
        PlatformServiceProvider.AudioDriver.PlayDrone(note, DroneVolume);
    }
    private void SuspendDrone()
    {
        _dronePlaying = false;
        PlatformServiceProvider.AudioDriver.ReleaseDrone();
    }
}
public record GameSettings(GameMode Mode = GameMode.Freeplay, bool RandomTonic = false, bool RandomOctave = false, int MelodyNoteCount = 2, bool PlayCadenceOnKeyChange = true, bool PlayDrone = true);
public enum GameMode
{
    Freeplay,
    Pocketmode,
    Interactive,
    Freelisten,
    Cycle,
    Melody
}
public enum AnswerState
{
    Correct,
    Neutral,
    Incorrect
}
public class MelodyBarState
{
    public Dictionary<int, string> UserChoices
    {
        get;
    } = new();

    public List<int> IncorrectChoices = new();
    public MelodyBarState(Dictionary<int, string> userChoices, List<int> incorrectChoices)
    {
        UserChoices = userChoices;
        IncorrectChoices = incorrectChoices;
    }
    public MelodyBarState()
    { }
}

public class InteractiveModeStats : IGameModeStats
{
    public TimeSpan AverageResponseTime = TimeSpan.Zero;
    public Dictionary<string, InteractiveDegreeStats> DegreeStats { get; private set; } = new();

    public void AddStatForDeg(string deg, TimeSpan responseTime, bool correct, string? mistakenFor)
    {
        _ = DegreeStats.TryAdd(deg, new());
        DegreeStats[deg].TotalResponseTime += responseTime;
        if (correct)
            DegreeStats[deg].TimesCorrect += 1;
        else
            DegreeStats[deg].TimesIncorrect += 1;
        if (mistakenFor is not null)
        {
            if (!DegreeStats[deg].MistakenFor.Contains(mistakenFor))
            {
                DegreeStats[deg].MistakenFor.Add(mistakenFor);
            }
        }
    }

}
public class InteractiveDegreeStats
{
    public int TimesCorrect { get; set; }
    public int TimesIncorrect { get; set; }
    public int TimesAppeared => TimesCorrect + TimesIncorrect; //will never be 0
    public TimeSpan TotalResponseTime { get; set; } = TimeSpan.Zero;
    public TimeSpan AverageResponseTime => TotalResponseTime / TimesAppeared;
    public List<string> MistakenFor = new();
}

public interface IGameModeStats { }


