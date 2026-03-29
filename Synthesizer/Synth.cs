namespace Synthesizer;

//TODO move this to main avalonia project
public interface IAudioDriver
{

    void Play(int note);

    void Release(int note);

    void PlayDrone(int note);

    void ReleaseDrone();
}

