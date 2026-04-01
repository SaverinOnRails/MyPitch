using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System;
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

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private bool _wideLayout;

    private Key _tonic = Key.C;
    private bool _shouldSelectAllDegrees = true;

    public bool ShouldSelectAllDegrees
    {
        get => _shouldSelectAllDegrees;
        set
        {
            SetProperty(ref _shouldSelectAllDegrees, value);
            foreach (var deg in Degrees)
            {
                deg.IsSelected = value;
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
        set { SetProperty(ref _tonic, value); ServiceProvider.AudioDriver.ReleaseDrone(); IsPlaying = false; }
    }

    public GameMode[] GameModes => new GameMode[] { GameMode.Freeplay, GameMode.Interactive, GameMode.Pocketmode };

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
        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            var note = MusicTheory.ToMidiNote(Tonic.ToString(), Tonic.ToString()); //wtf honestly
            ServiceProvider.AudioDriver.PlayDrone(note);
        }
        else
        {
            ServiceProvider.AudioDriver.ReleaseDrone();
        }
    }
}
public partial class DegreeItem : ObservableObject
{
    [ObservableProperty]
    private string label = "";


    [ObservableProperty]
    private bool isSelected = true;
}

