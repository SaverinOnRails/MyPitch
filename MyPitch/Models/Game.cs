using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MyPitch.Controls;
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
    [ObservableProperty] private int _interactiveModeRounds = 1;
    [ObservableProperty] private TimeSpan _folkMediaDuration;
    [ObservableProperty] private TimeSpan _folkMediaProgress;
    [ObservableProperty] private float _playbackSpeed = 1;

    // We can just change this reference to alert the view instead of implementing some change notifiers for its properties
    [ObservableProperty] private MelodyBarState _melodyBarState = new();

    public event DialogRequestedEventHandler? DialogNeeded;
    private const int _interactiveModeMinRoundCount = 20;
    private string? _currentInteractiveQuizDegree = null;
    private int _currentFolkNoteEventIndex = 0;
    private List<string>? _currentMelodyQuizDegrees = null;
    private bool _playedCadence;
    private const int GameClickTimeout = 500; // ms
    private CancellationTokenSource _gameCancellationTokenSource = new();
    private MelodyFile? _melodyFile = null;
    private CancellationTokenSource _repeatCancellationTokenSource = new();
    private TaskCompletionSource<int>? _userClickTcs;
    private Models.Key _oldTonic;
    private Stopwatch _folkMediaTimer = new();
    private double _folkMediaStartTimeMs = 0;
    private byte[]? _melodyFileBuffer = null;
    private bool _invalidateOldFolkMediaPLaybackLoop = false;
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
    public IEnumerable<MultiSelectableItem> AllowDegrees
    {
        get;
        set;
    } = new ObservableCollection<MultiSelectableItem>();
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
        _gameCancellationTokenSource.Cancel();
        _repeatCancellationTokenSource.Cancel();
        SuspendDrone();
        IsPlaying = false;
        _folkMediaTimer.Reset();
        _melodyFile = null;
        AnswerState = AnswerState.Neutral;
        FolkMediaDuration = TimeSpan.Zero;
        FolkMediaProgress = TimeSpan.Zero;
        InteractiveModeRounds = 1;
        _currentInteractiveQuizDegree = null;
        _currentFolkNoteEventIndex = 0;
        _currentMelodyQuizDegrees = null;
        MelodyBarState = new();
        _playedCadence = false;
        GameClickedIndex = null;
    }

    private async Task Start()
    {
        _gameCancellationTokenSource = new CancellationTokenSource();
        try
        {
            if (Settings.PlayDrone) PlayDrone();
            IsPlaying = true;
            await (Settings.Mode
                switch
            {
                GameMode.Interactive => InteractiveGameLoop(),
                // GameMode.Freelisten => FreeListenGameLoop(),
                GameMode.Pocketmode => PocketModeGameLoop(),
                GameMode.Cycle => CycleModeGameLoop(),
                GameMode.Melody => MelodyGameModeLoop(),
                // GameMode.Folk => FolkModeGameLoop(),
                _ => Task.CompletedTask // Freeplay
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            MaybeChangeTonic();
            MaybeChangeOctave();
            await MaybePlayCadence();
            await Task.Delay(GameClickTimeout * 2, _gameCancellationTokenSource.Token);
            var degrees = AllowedDegreeStrings;
            var melodyNoteCount = Settings.MelodyNoteCount;
            if (degrees.Count == 0) return;
            var melody = MusicTheory.GenMelody(degrees, melodyNoteCount);
            foreach (var note in melody)
            {
                await PlayScaleNote(note, hidden: true, Settings.MelodyNoteGapMs());
            }
            _currentMelodyQuizDegrees = melody;
            
            //await user responses
            List<string> userResponses = new();
            for (int i = 0; i < melodyNoteCount; i++)
            {
                _userClickTcs = new TaskCompletionSource<int>();
                var userResponse = await _userClickTcs.Task.WaitAsync(_gameCancellationTokenSource.Token);
                var dict = MelodyBarState.UserChoices;
                var deg = MusicTheory.FifthIntervalScaleGraduation[userResponse];
                dict[i] = deg;
                userResponses.Add(deg);
                MelodyBarState = new(dict, new());
            }
            _currentMelodyQuizDegrees = null;
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
                await Task.Delay(1000, _gameCancellationTokenSource.Token);
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                MelodyBarState = new(MelodyBarState.UserChoices, incorrectDegs);
                await Task.Delay(300, _gameCancellationTokenSource.Token);
                for (var i = 0; i < melodyNoteCount; i++)
                {
                    var dict = MelodyBarState.UserChoices;
                    dict[i] = melody[i];
                    MelodyBarState = new(dict, incorrectDegs);
                    await PlayScaleNote(melody[i], hidden: false, duration: 1000);
                    await Task.Delay(500, _gameCancellationTokenSource.Token);
                }
            }
        }
    }

    partial void OnPlaybackSpeedChanging(float oldValue, float newValue)
    {
        var currentMelodyTime = _folkMediaTimer.ElapsedMilliseconds * oldValue
                                  + _folkMediaStartTimeMs;
        _folkMediaStartTimeMs = currentMelodyTime;
        _folkMediaTimer.Restart();
    }
    private async Task FolkModeGameLoop()
    {
        if (_melodyFileBuffer is null) { Stop(); return; }
        _melodyFile = MelodyFile.FromBytes(_melodyFileBuffer);
        if (_melodyFile is null) { Stop(); return; }

        FolkMediaDuration = TimeSpan.FromMilliseconds(_melodyFile.DurationMs);
        _folkMediaStartTimeMs = 0;
        _invalidateOldFolkMediaPLaybackLoop = false;
        _folkMediaTimer.Restart();

        double lastNoteStopAt = -1;
        int lastNote = -1;

        while (true)
        {
            var now = _folkMediaTimer.ElapsedMilliseconds * PlaybackSpeed + _folkMediaStartTimeMs;

            // We've reached the end — wait out the remaining melody time then stop
            if (_currentFolkNoteEventIndex == _melodyFile.NoteEvents.Count())
            {
                var timeLeft = _melodyFile.DurationMs - now;
                while (timeLeft > 0)
                {
                    var chunk = Math.Min(timeLeft, 10);
                    await Task.Delay(
                        TimeSpan.FromMilliseconds(chunk / PlaybackSpeed),
                        _gameCancellationTokenSource.Token);

                    now = _folkMediaTimer.ElapsedMilliseconds * PlaybackSpeed + _folkMediaStartTimeMs;
                    FolkMediaProgress = TimeSpan.FromMilliseconds(now);
                    timeLeft = _melodyFile.DurationMs - now;
                }
                Stop();
                return;
            }

            var note = _melodyFile.NoteEvents[_currentFolkNoteEventIndex];
            var delay = note.TriggerAt - now;

            while (delay > 0)
            {
                if (_invalidateOldFolkMediaPLaybackLoop)
                {
                    _invalidateOldFolkMediaPLaybackLoop = false;
                    break;
                }
                if (lastNote != -1 && now >= lastNoteStopAt)
                {
                    PlatformServiceProvider.AudioDriver.Release(lastNote);
                    lastNote = -1;
                    lastNoteStopAt = -1;
                }

                var chunk = Math.Min(delay, 10);
                await Task.Delay(
                    TimeSpan.FromMilliseconds(chunk / PlaybackSpeed),
                    _gameCancellationTokenSource.Token);

                now = _folkMediaTimer.ElapsedMilliseconds * PlaybackSpeed + _folkMediaStartTimeMs;
                FolkMediaProgress = TimeSpan.FromMilliseconds(now);
                delay = note.TriggerAt - now;
            }
            var deg = note.ScaleDegree;
            //get the note in the original key
            var originalTonic = Enum.Parse<Key>(_melodyFile.OriginalTonic);
            var noteAtDeg = MusicTheory.NoteAtDegree(
                originalTonic,
                MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1,
                false);
            var noteToPlay = MusicTheory.ToMidiNote(noteAtDeg, note.Octave);
            Octave = note.Octave;
            GameClickedIndex = MusicTheory.FifthSegment(originalTonic, noteAtDeg);

            //now transpose to the user chosen tonic with the semitone offset
            noteToPlay += MusicTheory.ChromaticScale.IndexOf(Tonic.ToString()) - MusicTheory.ChromaticScale.IndexOf(_melodyFile.OriginalTonic);
            PlatformServiceProvider.AudioDriver.Play(noteToPlay);
            lastNoteStopAt = note.TriggerAt + note.DurationMs;
            lastNote = noteToPlay;
            _currentFolkNoteEventIndex++;
        }
    }
    private async Task CycleModeGameLoop()
    {
        int cycleIndex = 0;
        while (true)
        {
            int length = MusicTheory.ChromaticScaleGraduation.Length;
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            MaybeChangeOctave();
            while (!AllowedDegreeStrings.Contains(MusicTheory.ChromaticScaleGraduation[cycleIndex]))
            {
                if (AllowedDegreeStrings.Count() == 0) break;
                _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
                cycleIndex = (cycleIndex + 1) % length;
            }

            //only randomize when we complete a cycle
            if (AllowedDegreeStrings.Count > 0 && cycleIndex == MusicTheory.ChromaticScaleGraduation.IndexOf(AllowedDegreeStrings.First()))
            {
                MaybeChangeTonic();
                await MaybePlayCadence();
            }
            string degAtCycleIndex = MusicTheory.ChromaticScaleGraduation[cycleIndex];
            await PlayScaleNote(degAtCycleIndex, hidden: false, duration: 2000);
            cycleIndex = (cycleIndex + 1) % length;
            await Task.Delay(200, _gameCancellationTokenSource.Token);
        }
    }
    private async Task PocketModeGameLoop()
    {
        while (true)
        {
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            MaybeChangeTonic();
            MaybeChangeOctave();
            await MaybePlayCadence();
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _gameCancellationTokenSource.Token);
            var quizDeg = await PlayQuizNote(hidden: true);
            await Task.Delay(1000, _gameCancellationTokenSource.Token);
            PlatformServiceProvider.AudioDriver.PlaySpeechSample(quizDeg);
            await Task.Delay(1000, _gameCancellationTokenSource.Token);
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
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            MaybeChangeTonic();
            MaybeChangeOctave();
            await MaybePlayCadence();
            await Task.Delay(GameClickTimeout * 2, _gameCancellationTokenSource.Token);
            var quizDeg = await PlayQuizNote(hidden: true);
            var quizNoteIndex = MusicTheory.FifthSegment(tonic, MusicTheory.NoteAtDegree(tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(quizDeg) + 1, false));
            _userClickTcs = new TaskCompletionSource<int>();
            var responseStart = DateTime.Now;

            _currentInteractiveQuizDegree = quizDeg; //for repeat
            var userResponse = await _userClickTcs.Task.WaitAsync(_gameCancellationTokenSource.Token);
            _currentInteractiveQuizDegree = null;
            var responseEnd = DateTime.Now - responseStart;
            totalResponseTime += responseEnd;
            if (userResponse == quizNoteIndex)
            {
                AnswerState = AnswerState.Correct;
                GameClickedIndex = quizNoteIndex;
                await Task.Delay(1000, _gameCancellationTokenSource.Token);
                //resolve
                if (MusicTheory.SimpleResolutionMap.ContainsKey(quizDeg))
                {
                    var resolution = MusicTheory.SimpleResolutionMap[quizDeg];
                    await PlayScaleNote(resolution.ResolveTo, hidden: false, duration: 500, resolution.ResolveToNextOctave ? Octave + 1 : Octave);
                }
                stats.AddStatForDeg(quizDeg, responseEnd, true, null);
                GameClickedIndex = null;
            }
            else
            {
                AnswerState = AnswerState.Incorrect;
                await Task.Delay(400, _gameCancellationTokenSource.Token);
                for (var i = 0; i < 5; i++)
                {
                    await PlayScaleNote(quizDeg, hidden: false, duration: 200);
                    await Task.Delay(50, _gameCancellationTokenSource.Token);
                }
                stats.AddStatForDeg(quizDeg, responseEnd, false, MusicTheory.FifthIntervalScaleGraduation[userResponse]);
            }

            if (InteractiveModeRounds == _interactiveModeMinRoundCount)
            {
                stats.AverageResponseTime = ((totalResponseTime) / InteractiveModeRounds);
                DialogNeeded?.Invoke(new(
                    new InteractiveModeStatsDialogContent()
                    {
                        Stats = stats
                    }
                ));
                _gameCancellationTokenSource.Cancel();
            }
            InteractiveModeRounds++;
        }
    }
    private async Task FreeListenGameLoop()
    {
        while (true)
        {
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            MaybeChangeTonic();
            MaybeChangeOctave();
            await MaybePlayCadence();
            _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
            await Task.Delay(GameClickTimeout * 2, _gameCancellationTokenSource.Token);
            await PlayQuizNote(hidden: false);
            await Task.Delay(1000, _gameCancellationTokenSource.Token);
        }
    }

    public async Task TryRepeatQuizAsync()
    {
        if (!IsPlaying) return;
        if (Settings.Mode == GameMode.Interactive && _currentInteractiveQuizDegree is not null)
        {
            await PlayScaleNote(_currentInteractiveQuizDegree, true);
        }
        else if (Settings.Mode == GameMode.Melody && _currentMelodyQuizDegrees is not null)
        {
            //cancel any previous repeat attempts if any
            _repeatCancellationTokenSource.Cancel();
            _repeatCancellationTokenSource = new();
            foreach (var note in _currentMelodyQuizDegrees)
            {
                await PlayScaleNote(note, hidden: true, Settings.MelodyNoteGapMs(), cts: _repeatCancellationTokenSource);
            }
        }
        else { }
    }
    private void MaybeChangeTonic()
    {
        if (Settings.RandomTonic) Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
        if (Settings.PlayCadenceOnKeyChange && _oldTonic != Tonic)
        {
            _playedCadence = false;
        }
        _oldTonic = Tonic;
    }
    private void MaybeChangeOctave()
    {
        if (Settings.RandomOctave)
        {
            int[] octaveRange = [3, 4, 5];
            Octave = octaveRange[Random.Shared.Next(octaveRange.Length)];
        }
    }
    private async Task MaybePlayCadence()
    {
        if (!_playedCadence) await PlayCadence();
    }
    private async Task<string> PlayQuizNote(bool hidden)
    {
        _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
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
    private async Task PlayScaleNote(string deg, bool hidden, int duration = 500, int octave = -1, CancellationTokenSource? cts = null)
    {
        //use game cancellation token source unless another is specified
        var ctx = cts == null ? _gameCancellationTokenSource : cts;
        _gameCancellationTokenSource.Token.ThrowIfCancellationRequested();
        var noteAtDeg = MusicTheory.NoteAtDegree(Tonic, MusicTheory.ChromaticScaleGraduation.IndexOf(deg) + 1, false);
        var note = MusicTheory.ToMidiNote(Tonic, noteAtDeg, octave == -1 ? Octave : octave);
        if (!hidden) GameClickedIndex = MusicTheory.FifthSegment(Tonic, noteAtDeg);
        PlatformServiceProvider.AudioDriver.Play(note);
        try
        {
            await Task.Delay(duration, ctx.Token);
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

    public void FolkMediaSeek(double location)
    {
        if (_melodyFile is null) return;
        PlatformServiceProvider.AudioDriver.ReleaseAll();
        var indexAtLocation = _melodyFile.NoteEvents
            .Select((item, index) => new { item, index })
            .Where(x => x.item.TriggerAt > location)
            .Select(x => x.index)
            .FirstOrDefault();
        _currentFolkNoteEventIndex = indexAtLocation;
        _folkMediaStartTimeMs = location;
        _invalidateOldFolkMediaPLaybackLoop = true;
        _folkMediaTimer.Restart();
    }

    public void SetFolkModeMelodyFile(string path)
    {
        var buffer = EmbeddedResources.GetMelodyFile(path);
        _melodyFileBuffer = buffer;
    }
}
public record GameSettings(GameMode Mode = GameMode.Freeplay, bool RandomTonic = false, bool RandomOctave = false, int MelodyNoteCount = 2, bool PlayCadenceOnKeyChange = true, bool PlayDrone = true, int MelodyNoteGap = 1)
{
    public int MelodyNoteGapMs()
    {
        return 1000 * MelodyNoteGap;
    }
}
public enum GameMode
{
    Freeplay,
    Pocketmode,
    Interactive,
    Melody,
    ChordQuality,
    Cycle,
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

public class InteractiveModeStats
{
    public TimeSpan AverageResponseTime = TimeSpan.Zero;
    public Dictionary<string, InteractiveDegreeStats> DegreeStats { get; private set; } = new();
    public float Accuracy
    {
        get
        {
            var correct = DegreeStats.Sum(p => p.Value.TimesCorrect);
            var incorrect = DegreeStats.Sum(p => p.Value.TimesIncorrect);
            var total = correct + incorrect;
            if (total == 0)
                return 0;
            return (float)correct / total * 100f;
        }
    }
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

    public float Familiarity
    {
        get
        {
            if (TimesCorrect == 0) return 0f;
            var incorrectPenalty = 1f / (1f + TimesIncorrect);
            var confusionPenalty = 1f / (1f + MistakenFor.Count());

            float responseTimeFactor = 0F;
            if (AverageResponseTime.TotalSeconds < 1)
            {
                responseTimeFactor = 1f;
            }
            else if (AverageResponseTime.TotalSeconds < 2)
            {
                responseTimeFactor = 0.5f;
            }
            else
            {
                responseTimeFactor = 0.2f;
            }
            return incorrectPenalty * 0.6f + confusionPenalty * 0.3f + responseTimeFactor * 0.1f;
        }
    }
}
