using MeltySynth;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Synthesizer;

public interface IAudioDriver
{

    void Play(int note);

    void Release();
}

