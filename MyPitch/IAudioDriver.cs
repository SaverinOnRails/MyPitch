using System;
using System.Collections.Generic;
using System.Text;

namespace MyPitch;

public interface IAudioDriver
{

    void Play(int note);

    void Release(int note);

    void PlayDrone(int note);

    void ReleaseDrone();
}

