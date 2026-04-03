
using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public bool _isPlaying = false;
    public MainViewModel()
    {
        _game.PlayingStatusChanged += _game_PlayingStatusChanged;
        _game.GameClickedIndexChanged += _game_GameClickedIndexChanged;
        _game.AnswerStateChanged += _game_AnswerStateChanged;
        _game.AllowDegrees = Degrees;
        foreach (var deg in Degrees)
        {
            deg.PropertyChanged += Deg_PropertyChanged;
        }
    }

    private void _game_AnswerStateChanged(object? sender, System.EventArgs e)
    {
        AnswerState = _game.AnswerState;
    }

    private void Deg_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _game.AllowDegrees = Degrees;
    }

    private void _game_GameClickedIndexChanged(object? sender, System.EventArgs e)
    {
        GameClickedIndex = _game.GameClickedIndex;
    }

    private void _game_PlayingStatusChanged(object? sender, System.EventArgs e)
    {
        IsPlaying = _game.IsPlaying;
    }

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private int _octave = 4;

    [ObservableProperty]
    private int? _gameClickedIndex = null;

    [ObservableProperty]
    private AnswerState _answerState;

    [ObservableProperty]
    private bool _wideLayout;

    private GameMode _gameMode = GameMode.Freeplay;

    public GameMode GameMode
    {
        get => _gameMode;
        set { SetProperty(ref _gameMode, value); _game.Mode = value; }
    }
    private Key _tonic = Key.C;
    private bool _shouldSelectAllDegrees = true;
    private bool _shouldSelectMajorScale = true;
    private int? _userClickedIndex = null;
    private Game _game = new();

    public bool ShouldSelectAllDegrees
    {
        get => _shouldSelectAllDegrees;
        set
        {
            SetProperty(ref _shouldSelectAllDegrees, value);
            foreach (var deg in Degrees)
            {
                if (deg.Label.Length == 1 && _shouldSelectMajorScale == true)
                {
                    deg.IsSelected = true;
                }
                else
                {
                    deg.IsSelected = value;
                }
            }

        }
    }
    public int? UserClickedIndex
    {
        get => _userClickedIndex;
        set
        {
            SetProperty(ref _userClickedIndex, value);
            _game.UserClickedIndex = value;
        }
    }
    public bool ShouldSelectMajorScale
    {
        get => _shouldSelectMajorScale;
        set
        {
            SetProperty(ref _shouldSelectMajorScale, value);
            foreach (var deg in Degrees)
            {
                if (deg.Label.Length == 1) deg.IsSelected = value; //no 'flats' i.e just numbers  means it's a member of the major scale
            }
        }
    }
    public ObservableCollection<DegreeItem> Degrees { get; } =
    [
        new() { Label = "1" },
        new() { Label = "♭2" },
        new() { Label = "2" },
        new() { Label = "♭3" },
        new() { Label = "3" },
        new() { Label = "4" },
        new() { Label = "#4" },
        new() { Label = "5" },
        new() { Label = "♭6" },
        new() { Label = "6" },
        new() { Label = "♭7" },
        new() { Label = "7" }
    ];
    public Key Tonic
    {

        get => _tonic;
        set { SetProperty(ref _tonic, value); _game.Tonic = value; }
    }

    public GameMode[] GameModes => new GameMode[] { GameMode.Freeplay, GameMode.Interactive, GameMode.Pocketmode, GameMode.Freelisten };

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            SetProperty(ref _isPlaying, value);
        }
    }

    //TODO: doing this over and over again is retarded
    public Key[] Tonics => new Key[] { Key.C, Key.Dflat, Key.D, Key.Eflat, Key.E, Key.F, Key.Gflat, Key.G, Key.Aflat, Key.A, Key.Bflat, Key.B };
    public void TogglePlay()
    {
        _ = _game.TogglePlay();

    }
}
public partial class DegreeItem : ObservableObject
{
    [ObservableProperty]
    private string label = "";


    [ObservableProperty]
    private bool isSelected = true;
}

