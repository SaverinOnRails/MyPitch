using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Controls;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public Game Game { get; } = new();
    public DialogHost? _dialogHost;
    private IMidiInput? _midiInput;
    private byte _midiStatus;
    private int _midiFirstDataByte = -1;
    public List<MultiSelectableItem<string>> Degrees { get; } =
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

    public List<MultiSelectableItem<ChordQuality>> ChordQualities { get; } = Enum
        .GetValues<ChordQuality>()
        .Select(q =>
        {
            var item = new MultiSelectableItem<ChordQuality>
            {
                Label = q.ToString(),
                Data = q,
                IsSelected = q == ChordQuality.Major || q == ChordQuality.Minor
            };
            return item;
        })
        .ToList();


    public bool IsMelodyMode => GameMode == GameMode.Melody;
    public bool IsInteractiveMode => GameMode == GameMode.Interactive || GameMode == GameMode.Diatonics;
    public bool IsFolkMode => false;  //GameMode == GameMode.Folk;
    public bool IsChordQualityMode => GameMode == GameMode.ChordQuality;

    public bool HideCircleOfFifths => GameMode == GameMode.ChordQuality;
    public bool GameModeNeedTonicControl => true;
    public bool GameModeNeedDroneControl => GameMode != GameMode.ChordQuality;
    public bool GameModeNeedOctaveControl => GameMode != GameMode.ChordQuality;
    public bool GameModeNeedScaleControl => GameMode != GameMode.ChordQuality;
    public bool ShowMidiInput =>
        GameModeNeedScaleControl
            && !IsWasm;

    private static bool RequiresStrictScaleDegreeAdherence(GameMode? mode) =>
        mode is GameMode.Melody
            or GameMode.Diatonics;

    public bool ActiveGameModeNeedStrictScaleDegreeAdherence =>
        RequiresStrictScaleDegreeAdherence(Game.CurrentGameMode) ||
        RequiresStrictScaleDegreeAdherence(GameMode);

    public Key[] Tonics => MusicTheory.Keys;
    public GameMode[] GameModes => Enum.GetValues<GameMode>();
    public ScaleMode[] ScaleModes => Enum.GetValues<ScaleMode>();
    public IMidiPortDetails[] MidiInputDevices =>
        IsWasm
            ? Array.Empty<IMidiPortDetails>()
            : MidiAccessManager.Default.Inputs.ToArray();


    [ObservableProperty] private bool _wideLayout;
    [ObservableProperty] private bool _shouldSelectAllDegrees = true;
    [ObservableProperty] private int _melodyNoteCount = 3;
    [ObservableProperty] private int _melodyNoteGap = 1;
    [ObservableProperty] private GameMode _gameMode = GameMode.Freeplay;
    [ObservableProperty] private bool _useRandomTonic;
    [ObservableProperty] private bool _useRandomOctave;
    [ObservableProperty] private bool _playCadenceOnKeyChange = true;
    [ObservableProperty] private bool _resolveAfterRound = true;
    [ObservableProperty] private bool _playDrone = true;
    [ObservableProperty] private IMidiPortDetails? _selectedMidiInputDevice;
    [ObservableProperty] private int? _midiPressedIndex;
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
        if (newValue == GameMode.Melody || newValue == GameMode.Diatonics) ConfigureDiatonic();
        EnsureEnabledDegrees();
        OnPropertyChanged(nameof(ShowMidiInput));
    }

    private void EnsureEnabledDegrees()
    {
        if (!ActiveGameModeNeedStrictScaleDegreeAdherence)
        {
            foreach (var degree in Degrees)
                degree.IsEnabled = true;

            return;
        }

        var allowed = MusicTheory.DegsForScaleMode(Game.ScaleMode).ToHashSet();
        foreach (var degree in Degrees)
        {
            var enabled = allowed.Contains(degree.Label);

            degree.IsEnabled = enabled;

            if (!enabled)
                degree.IsSelected = false;
        }
    }

    partial void OnMelodyNoteCountChanged(int oldValue, int newValue) => PushSettings();
    partial void OnMelodyNoteGapChanged(int oldValue, int newValue) => PushSettings();

    private void ConfigureDiatonic()
    {
        SetScaleMode(Game.ScaleMode);
    }

    void OnScaleModeChanged(ScaleMode value) => SetScaleMode(value);

    private void SetScaleMode(ScaleMode value)
    {
        var degs = MusicTheory.DegsForScaleMode(value);
        foreach (var x in Degrees)
        {
            x.IsSelected = degs.Contains(x.Label);
        }
        EnsureEnabledDegrees();
    }

    partial void OnUseRandomTonicChanged(bool value)
    {
        PushSettings(); if (value) SetRandomTonicManual();
    }

    partial void OnResolveAfterRoundChanged(bool value)
    {
        PushSettings();
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
        {
            deg.PropertyChanged += (_, _) => SyncDegrees();
        }
        foreach (var qual in ChordQualities)
        {
            qual.PropertyChanged += (_, _) => SyncChordQualities();
        }
        Game.DialogNeeded += GameDialogNeeded;
        Game.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Game.ScaleMode)) OnScaleModeChanged(Game.ScaleMode);
        };
        PushSettings();
        SyncDegrees();
        SyncChordQualities();
    }

    private void ScaleDegreeSelectableChanging(PropertyChangingEventArgs e)
    {
        if (!ActiveGameModeNeedStrictScaleDegreeAdherence) return;
    }

    private void GameDialogNeeded(DialogRequestedEventArgs e)
    {
        _dialogHost?.Show(e.Content, this);
    }

    partial void OnShouldSelectAllDegreesChanged(bool value)
    {
        foreach (var deg in Degrees)
        {
            deg.IsSelected = deg.IsEnabled;
        }
    }

    async partial void OnSelectedMidiInputDeviceChanged(IMidiPortDetails? value)
    {
        try
        {
            if (_midiInput is not null)
            {
                await _midiInput.CloseAsync();
                _midiInput = null;
            }

            if (value is null) return;
            _midiInput = await MidiAccessManager.Default.OpenInputAsync(value.Id);
            Console.WriteLine($"Opened MIDI input: {value.Name}");

           _midiStatus = 0;
           _midiFirstDataByte = -1;

           _midiInput.MessageReceived += (sender, e) =>
           {
               for (int i = e.Start; i < e.Start + e.Length; i++)
               {
                   ProcessMidiByte(e.Data[i]);
               //    Console.WriteLine(BitConverter.ToString(e.Data, e.Start, e.Length));
               }
           };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open MIDI input: {ex.Message}");
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

    private void PushSettings() => Game.ApplySettings(new GameSettings(GameMode, UseRandomTonic, UseRandomOctave, MelodyNoteCount, PlayCadenceOnKeyChange, PlayDrone, MelodyNoteGap, ResolveAfterRound));

    private void SyncDegrees() => Game.AllowedDegrees = Degrees;
    private void SyncChordQualities() => Game.AllowedChordQualities = ChordQualities;

    private void AnswerMidiNote(int noteNumber)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            string note = MusicTheory.ChromaticScale[noteNumber % 12];

            int index = MusicTheory.GetCircleOfFifthsIndexFromNote(
                Game.Tonic, note);
	    PlatformServiceProvider.AudioDriver.Play(noteNumber);
            MidiPressedIndex = index;
            Game.UserResponse = new CircleIndexResponse(index);
        });
    }

    private void ReleaseMidiNote(int noteNumber)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            PlatformServiceProvider.AudioDriver.Release(noteNumber);

            string note = MusicTheory.ChromaticScale[noteNumber % 12];
            int index = MusicTheory.GetCircleOfFifthsIndexFromNote(
                Game.Tonic, note);

            if (MidiPressedIndex == index)
            {
                MidiPressedIndex = null;
            }
        });
    }

private void ProcessMidiByte(byte value)
{
        // Timing messages can appear anywhere; skip them.
        if (value >= 0xF8) return;

        // A status byte starts a message.
        if ((value & 0x80) != 0)
        {
            _midiStatus = value < 0xF0 ? value : (byte)0;
            _midiFirstDataByte = -1;
            return;
        }

        if (_midiStatus == 0) return;

        int messageType = _midiStatus & 0xF0;

        // These message types have one data byte and aren't note presses.
        if (messageType == 0xC0 || messageType == 0xD0) return;

        if (_midiFirstDataByte == -1)
        {
            _midiFirstDataByte = value;
            return;
        }

        int noteNumber = _midiFirstDataByte;
        int velocity = value;
        _midiFirstDataByte = -1;

        if (messageType == 0x90 && velocity > 0)
        {
            AnswerMidiNote(noteNumber);
        }
        else if (messageType == 0x80 ||
                 (messageType == 0x90 && velocity == 0))
        {
            ReleaseMidiNote(noteNumber);
        }

    }



}
public partial class MultiSelectableItem<TData> : ObservableObject
{
    public string Label { get; set; } = "";
    [ObservableProperty] private bool _isSelected = true;
    [ObservableProperty] private bool _isEnabled = true;
    public TData? Data { get; set; }

}

