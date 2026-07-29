
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using MyPitch.Models;
using MyPitch.ViewModels;

namespace MyPitch.Controls;

internal abstract class GameInputControl<TMultiSelectable> : ContentControl
{
    public static readonly StyledProperty<IModeResponse?> GameResponseProperty = AvaloniaProperty.Register<GameInputControl<TMultiSelectable>, IModeResponse?>(nameof(UserResponse), null);

    public static readonly StyledProperty<IModeResponse?> UserResponseProperty = AvaloniaProperty.Register<GameInputControl<TMultiSelectable>, IModeResponse?>(nameof(UserResponse), null);

    public static readonly StyledProperty<AnswerState> AnswerStateProperty = AvaloniaProperty.Register<GameInputControl<TMultiSelectable>, AnswerState>(nameof(AnswerState));

    public static readonly StyledProperty<Models.Key> TonicProperty = AvaloniaProperty.Register<GameInputControl<TMultiSelectable>, Models.Key>(nameof(Tonic));

    public static readonly StyledProperty<IList<MultiSelectableItem<TMultiSelectable>>> IncludedMultiSelectableProperty = AvaloniaProperty.Register<GameInputControl<TMultiSelectable>, IList<MultiSelectableItem<TMultiSelectable>>>(nameof(IncludedMultiSelectable));

    public IList<MultiSelectableItem<TMultiSelectable>> IncludedMultiSelectable
    {
        get => GetValue(IncludedMultiSelectableProperty);
        set
        {
            SetValue(IncludedMultiSelectableProperty, value);
        }
    }

    public Models.Key Tonic
    {
        get => GetValue(TonicProperty);
        set
        {
            SetValue(TonicProperty, value);
        }
    }

    public AnswerState AnswerState
    {
        get => GetValue(AnswerStateProperty);
        set
        {
            SetValue(AnswerStateProperty, value);
        }
    }

    public IModeResponse? UserResponse
    {
        get => GetValue(UserResponseProperty);
        set => SetValue(UserResponseProperty, value);
    }

    public IModeResponse? GameResponse
    {
        get => GetValue(GameResponseProperty);
        set => SetValue(GameResponseProperty, value);
    }

    protected virtual void OnGameResponseChanged() { DrawFunc(); }
    protected virtual void OnAnswerStateChanged() { DrawFunc(); }
    protected virtual void OnMultiSelectableChanged() { DrawFunc(); }

    protected abstract void DrawFunc();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == GameResponseProperty)
        {
            OnGameResponseChanged();
            DrawFunc();
        }
        if (change.Property == AnswerStateProperty)
        {
            OnAnswerStateChanged();
            DrawFunc();
        }
        if (change.Property == IncludedMultiSelectableProperty)
        {
            if (change.NewValue is null) return;
            var value = (IList<MultiSelectableItem<TMultiSelectable>>)change.NewValue;
            foreach (var deg in value)
            {
                deg.PropertyChanged += IncludedMultiSelectableChanged;
            }
        }
        base.OnPropertyChanged(change);
    }

    private void IncludedMultiSelectableChanged(object? sender, PropertyChangedEventArgs e)
    {
        DrawFunc();
    }
}
