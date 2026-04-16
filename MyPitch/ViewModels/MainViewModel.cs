
using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public Game Game { get; } = new();

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
    [ObservableProperty] private bool _shouldSelectMajorScale = true;

    private GameMode _gameMode = GameMode.Freeplay;
    private bool _useRandomTonic;
    private bool _useRandomOctave;
    private bool _playCadenceOnKeyChange = true;
    private bool _playDrone = true;

    public bool IsWasm => OperatingSystem.IsBrowser();
    public GameMode GameMode
    {
        get => _gameMode;
        set { SetProperty(ref _gameMode, value); PushSettings(); }
    }

    public bool UseRandomTonic
    {
        get => _useRandomTonic;
        set { SetProperty(ref _useRandomTonic, value); PushSettings(); if(value) SetRandomTonicManual(); }
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
    public bool UseRandomOctave
    {
        get => _useRandomOctave;
        set { SetProperty(ref _useRandomOctave, value); PushSettings(); if(value) SetRandomOctaveManual(); }
    }
    public bool PlayDrone
    {
        get => _playDrone;
        set { SetProperty(ref _playDrone, value); PushSettings(); }
    }
    public bool PlayCadenceOnKeyChange
    {
        get => _playCadenceOnKeyChange;
        set { SetProperty(ref _playCadenceOnKeyChange, value); PushSettings(); }
    }


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
    public AnswerState AnswerState => Game.AnswerState;

    public Key[] Tonics => MusicTheory.Keys;
    public GameMode[] GameModes => [GameMode.Freeplay, GameMode.Interactive, GameMode.Pocketmode, GameMode.Freelisten , GameMode.Cycle];

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
            if (_shouldSelectMajorScale && IsMajorScaleDegree(deg))
                deg.IsSelected = true;
            else
                deg.IsSelected = value;
        }
    }

    partial void OnShouldSelectMajorScaleChanged(bool value)
    {
        foreach (var deg in Degrees)
            if (IsMajorScaleDegree(deg))
                deg.IsSelected = value;
    }

    public async Task TogglePlay() => await Game.TogglePlay();

    private void PushSettings() =>
        Game.ApplySettings(new GameSettings(_gameMode, _useRandomTonic, _useRandomOctave, _playCadenceOnKeyChange, _playDrone));

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