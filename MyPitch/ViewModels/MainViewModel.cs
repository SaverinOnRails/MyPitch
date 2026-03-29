using CommunityToolkit.Mvvm.ComponentModel;
using MyPitch.Models;
using System;
using System.Reflection;

namespace MyPitch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public bool _isPlaying = false;

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private Key _tonic = Key.C;

    [ObservableProperty]
    private string _playText = "Play";

    public bool IsPlaying
    {
        get => _isPlaying;
        set { SetProperty(ref _isPlaying, value); }
    }

    public void TogglePlay()
    {
        IsPlaying = !IsPlaying;
        PlayText = IsPlaying ? "Stop" : "Play";
        if (IsPlaying)
        {
            var note = MusicTheory.ToMidiNote(Tonic.ToString(),Tonic.ToString()); //wtf honestly
            ServiceProvider.AudioDriver.PlayDrone(note);
        }
        else
        {
            ServiceProvider.AudioDriver.ReleaseDrone();
        }
    }
}
