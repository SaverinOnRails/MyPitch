
using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Game Game { get; } = new();

    public ObservableCollection<DegreeItem> Degrees { get; } =
        [
        new() { Label = "1"  },
    new() { Label = "♭2" },
    new() { Label = "2"  },
    new() { Label = "♭3" },
    new() { Label = "3"  },
    new() { Label = "4"  },
    new() { Label = "#4" },
    new() { Label = "5"  },
    new() { Label = "♭6" },
    new() { Label = "6"  },
    new() { Label = "♭7" },
    new() { Label = "7"  },
    ];


    [ObservableProperty] private bool _wideLayout;
    [ObservableProperty] private bool _shouldSelectAllDegrees = true;
    [ObservableProperty] private int _melodyNoteCount = 3;
    [ObservableProperty] private GameMode _gameMode = GameMode.Freeplay;
    [ObservableProperty] private ScaleMode _scaleMode;
    [ObservableProperty] private bool _useRandomTonic;
    [ObservableProperty] private bool _useRandomOctave;
    [ObservableProperty] private bool _playCadenceOnKeyChange = true;
    [ObservableProperty] private bool _playDrone = true;
    public bool IsWasm => OperatingSystem.IsBrowser();

    partial void OnGameModeChanged(GameMode oldValue, GameMode newValue)
    {
        PushSettings(); OnPropertyChanged(nameof(IsMelodyMode)); if (newValue == GameMode.Melody) ConfigureMelodyMode();
    }
    partial void OnMelodyNoteCountChanged(int oldValue, int newValue) => PushSettings();

    private void ConfigureMelodyMode()
    {
        ScaleMode = ScaleMode.Ionian;
        SetScaleMode(ScaleMode);
    }

    partial void OnScaleModeChanged(ScaleMode value) => SetScaleMode(value);

    private void SetScaleMode(ScaleMode value)
    {
        var degs = MusicTheory.DegsForScaleMode(value);
        foreach (var x in Degrees)
        {
            x.IsSelected = degs.Contains(x.Label);
        }
    }

    partial void OnUseRandomTonicChanged(bool value)
    {
        PushSettings(); if (value) SetRandomTonicManual();
    }

    //THESE ONLY RUN WHEN THE USER MANUALLY TRIGGERS THEM IN THE UI
    private void SetRandomTonicManual()
    {
        Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
    }
    private void SetRandomOctaveManual()
    {
        int[] octaveRange = [3, 4, 5];
        Octave = octaveRange[Random.Shared.Next(octaveRange.Length)];
    }

    partial void OnUseRandomOctaveChanged(bool value)
    {
        PushSettings(); if (value) SetRandomOctaveManual();
    }

    partial void OnPlayDroneChanged(bool value) => PushSettings();


    partial void OnPlayCadenceOnKeyChangeChanged(bool value) => PushSettings();

    public bool IsMelodyMode => GameMode == GameMode.Melody;

    public Key Tonic
    {
        get => Game.Tonic;
        set => Game.Tonic = value;
    }

    public int Octave
    {
        get => Game.Octave;
        set => Game.Octave = value;
    }

    public int? UserClickedIndex
    {
        get => Game.UserClickedIndex;
        set => Game.UserClickedIndex = value;
    }


    public bool IsPlaying => Game.IsPlaying;
    public int? GameClickedIndex => Game.GameClickedIndex;
    public MelodyBarState MelodyBarState => Game.MelodyBarState;
    public AnswerState AnswerState => Game.AnswerState;

    public Key[] Tonics => MusicTheory.Keys;
    public GameMode[] GameModes => [GameMode.Freeplay, GameMode.Interactive, GameMode.Pocketmode, GameMode.Melody, GameMode.Cycle];
    public ScaleMode[] ScaleModes => [ScaleMode.Ionian, ScaleMode.Dorian, ScaleMode.Phrygian, ScaleMode.Lydian, ScaleMode.Mixolydian, ScaleMode.Aeolian, ScaleMode.Locrian];
    public MainViewModel()
    {
        Game.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);

        foreach (var deg in Degrees)
            WireDegree(deg);

        PushSettings();
        SyncDegrees();
    }

    partial void OnShouldSelectAllDegreesChanged(bool value)
    {
        foreach (var deg in Degrees)
        {
            deg.IsSelected = value;
        }
    }

    public async Task TogglePlay() => await Game.TogglePlay();

    private void PushSettings() =>
                                                                                                Game.ApplySettings(new GameSettings(GameMode, UseRandomTonic, UseRandomOctave, MelodyNoteCount, PlayCadenceOnKeyChange, PlayDrone));

    private void SyncDegrees() =>
                                                                                                    Game.AllowDegrees = Degrees;

    private void WireDegree(DegreeItem deg) =>
    deg.PropertyChanged += (_, _) => SyncDegrees();

    private static bool IsMajorScaleDegree(DegreeItem deg) => deg.Label.Length == 1;
}

public partial class DegreeItem : ObservableObject
{
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private bool _isSelected = true;
}
