using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public bool _isPlaying = false;

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    private Key _tonic = Key.C;
    public Key Tonic
    {
        get => _tonic;
        set { SetProperty(ref _tonic, value); ServiceProvider.AudioDriver.ReleaseDrone(); IsPlaying = false; }
    }

    [ObservableProperty]
    public string _playText = "Play";

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            SetProperty(ref _isPlaying, value);
            if (value)
            {
                PlayText = "Stop";
            }
            else
            {
                PlayText = "Play";
            }
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
