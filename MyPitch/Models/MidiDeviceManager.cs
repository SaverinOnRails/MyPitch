namespace MyPitch.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Music.Midi;

public class MidiDeviceManager
{
	private IMidiInput? _midiInput;
	private byte _midiStatus;
	private int _midiFirstDataByte = -1;

	private IMidiAccess? _access;
	public event EventHandler<MidiReceivedEventArgs>? MessageReceived;

	public MidiDeviceManager()
	{
	}

	public async void SetMidiInput(IMidiPortDetails? value)
	{
		try
		{
			if (_midiInput is not null)
			{
				await _midiInput.CloseAsync();

				_midiInput.MessageReceived -= OnMidiMessageReceived; _midiInput = null;
			}
			if (value is null) return;
			_midiInput = await MidiAccessManager.Default.OpenInputAsync(value.Id);
			Console.WriteLine($"Opened MIDI input: {value.Name}");
			_midiStatus = 0;
			_midiFirstDataByte = -1;
			_midiInput.MessageReceived += OnMidiMessageReceived;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Could not open MIDI input: {ex.Message}");
		}
	}
	public IMidiPortDetails[] GetMidiDevices()
	{
		return [.. MidiAccessManager.Default.Inputs];
	}

	private void OnMidiMessageReceived(object? sender, MidiReceivedEventArgs e)
	{
		MessageReceived?.Invoke(this, e);
	}
}

