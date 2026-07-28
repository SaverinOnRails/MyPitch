using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Controls;
using MyPitch.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public Game Game { get; } = new();
    public DialogHost? _dialogHost;
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

    public bool IsMelodyMode => GameMode == GameMode.Melody;
    public bool IsInteractiveMode => GameMode == GameMode.Interactive;
    public bool IsFolkMode => false;  //GameMode == GameMode.Folk;
    public bool IsChordQualityMode => GameMode == GameMode.ChordQuality;

    public bool HideCircleOfFifths => GameMode == GameMode.ChordQuality;
    public bool GameModeNeedTonicControl => GameMode != GameMode.ChordQuality;
    public bool GameModeNeedDroneControl => GameMode != GameMode.ChordQuality;
    public bool GameModeNeedOctaveControl => GameMode != GameMode.ChordQuality;
    public bool GameModeNeedScaleControl => GameMode != GameMode.ChordQuality;

    public Key[] Tonics => MusicTheory.Keys;
    public GameMode[] GameModes => Enum.GetValues<GameMode>();
    public ScaleMode[] ScaleModes => [ScaleMode.All, ScaleMode.Ionian, ScaleMode.Dorian, ScaleMode.Phrygian, ScaleMode.Lydian, ScaleMode.Mixolydian, ScaleMode.Aeolian, ScaleMode.Locrian];

    [ObservableProperty] private bool _wideLayout;
    [ObservableProperty] private bool _shouldSelectAllDegrees = true;
    [ObservableProperty] private int _melodyNoteCount = 3;
    [ObservableProperty] private int _melodyNoteGap = 1;
    [ObservableProperty] private GameMode _gameMode = GameMode.Freeplay;
    [ObservableProperty] private ScaleMode _scaleMode;
    [ObservableProperty] private bool _useRandomTonic;
    [ObservableProperty] private bool _useRandomOctave;
    [ObservableProperty] private bool _playCadenceOnKeyChange = true;
    [ObservableProperty] private bool _playDrone = true;
    public bool IsWasm => OperatingSystem.IsBrowser();

    partial void OnGameModeChanged(GameMode oldValue, GameMode newValue)
    {
        PushSettings();
        OnPropertyChanged(nameof(IsMelodyMode));
        OnPropertyChanged(nameof(IsInteractiveMode));
        OnPropertyChanged(nameof(IsFolkMode));
        OnPropertyChanged(nameof(IsChordQualityMode));
        OnPropertyChanged(nameof(HideCircleOfFifths));
        OnPropertyChanged(nameof(GameModeNeedDroneControl));
        OnPropertyChanged(nameof(GameModeNeedOctaveControl));
        OnPropertyChanged(nameof(GameModeNeedScaleControl));
        OnPropertyChanged(nameof(GameModeNeedTonicControl));
        if (newValue == GameMode.Melody) ConfigureMelodyMode();
    }
    partial void OnMelodyNoteCountChanged(int oldValue, int newValue) => PushSettings();
    partial void OnMelodyNoteGapChanged(int oldValue, int newValue) => PushSettings();

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

    private void SetRandomTonicManual()
    {
        Game.Tonic = MusicTheory.Keys[Random.Shared.Next(MusicTheory.Keys.Length)];
    }
    private void SetRandomOctaveManual()
    {
        int[] octaveRange = [3, 4, 5];
        Game.Octave = octaveRange[Random.Shared.Next(octaveRange.Length)];
    }

    partial void OnUseRandomOctaveChanged(bool value)
    {
        PushSettings(); if (value) SetRandomOctaveManual();
    }

    partial void OnPlayDroneChanged(bool value) => PushSettings();

    partial void OnPlayCadenceOnKeyChangeChanged(bool value) => PushSettings();

    public MainViewModel()
    {
        foreach (var deg in Degrees)
            WireDegree(deg);
        Game.DialogNeeded += GameDialogNeeded;
        PushSettings();
        SyncDegrees();
    }

    private void GameDialogNeeded(DialogRequestedEventArgs e)
    {
        _dialogHost?.Show(e.Content, this);
    }

    partial void OnShouldSelectAllDegreesChanged(bool value)
    {
        foreach (var deg in Degrees)
        {
            deg.IsSelected = value;
        }
    }
    public async Task Repeat()
    {
        await Game.TryRepeatQuizAsync();
    }
    public void FolkMediaSeek(object l)
    {
        double location = (double)l;
        Game.FolkMediaSeek(location);
    }
    public void OpenFoldDatabaseDialog()
    {
        _dialogHost?.Show(new FolkDatabaseDialogContent(), this);
    }
    public async Task TogglePlay() => await Game.TogglePlay();

    private void PushSettings() => Game.ApplySettings(new GameSettings(GameMode, UseRandomTonic, UseRandomOctave, MelodyNoteCount, PlayCadenceOnKeyChange, PlayDrone, MelodyNoteGap));

    private void SyncDegrees() => Game.AllowDegrees = Degrees;

    private void WireDegree(DegreeItem deg) =>
    deg.PropertyChanged += (_, _) => SyncDegrees();

}

public partial class DegreeItem : ObservableObject
{
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private bool _isSelected = true;
}
