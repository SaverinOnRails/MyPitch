# MyPitch
Simple and powerful cross platform Functional Ear Trainer
</br>

# About
MyPitch is a free, open-source, simple, and customizable ear trainer for musicians. It helps train your ability to recognize the functions of scale notes within a tonal context and, by extension, your ability to recognize these pitches.
Try in your browser:
[MyPitch](https://saverinonrails.github.io)
NOTE: while MyPitch does run in a browser, Performance is significantly better on the desktop or even Mobile apps. It is desktop first.

MyPitch is still early in dev so bugs can be expected.
</br>

<img width="1920" height="1013" alt="Screenshot (10)" src="https://github.com/user-attachments/assets/13f7a8c5-adfa-4391-883e-42dc096be56b" />
</br>
</br>

<img width="1920" height="1013" alt="Screenshot (9)" src="https://github.com/user-attachments/assets/2938fdd5-a232-4bf2-b52d-25f35078d99d" />
Interactive Mode
</br>
</br>

<img width="1915" height="1013" alt="Screenshot (11)" src="https://github.com/user-attachments/assets/f4b51f1d-80c5-43d9-9282-c1e8cd23ccfe" />
Performance review after interactive mode round highlighting areas that need improvement
</br>
</br>

<img width="1906" height="1010" alt="Screenshot (12)" src="https://github.com/user-attachments/assets/c2be5344-33b8-48a1-8b5d-3e71ba9a68f7" />
Melody mode, training the ability to obtain individual scale degrees from a short melody
</br>
</br>

<img width="1918" height="1001" alt="Screenshot (13)" src="https://github.com/user-attachments/assets/a151a07b-11c9-45e0-b190-7ecc42c285b7" />
Folk database with popular nursery rhymes, Hymns , Christman carols or just popular music
</br>
</br>

<img width="1911" height="1011" alt="Screenshot (14)" src="https://github.com/user-attachments/assets/160ec657-a74b-4a62-8772-503803d08110" />
Folk media playback, plays the scale degrees for folk songs showing how the work in real music. Playback speed is adjustable and folk songs are fully transposable into any key,
</br>
</br>



# Features
- Interactive quiz mode for all degrees or subsets at any key and octave, even randomized!
- Pocket mode with automatic speech responses for hands free pratice
- Customizable Tonic Drone for anchoring to the key.
- Melody quiz to drill decoding melodies within a key context in various scale modes.
- Stats report after interactive mode displaying useful data about strengths and weaknesses to better help you customize your next session.
- Folk mode to observe and recognise scale degrees in real music or melodies
# Building
To build from source on Desktop (Windows, Mac , Linux) install the latest dotnet 10 from dotnet.microsoft.com.
On Macos and Linux you must install FluidSynth from your respective package managers
```
git clone https://github.com/saverinonrails/mypitch MyPitch
cd MyPitch/MyPitch.Desktop
dotnet run
dotnet publish --output dist
```

Arch Linux users can install from the AUR:
```
yay -S mypitch-git
```

  
# How To Use
Max Konyi has great videos on the concept this program is based on on youtube. Check him out: https://www.youtube.com/watch?v=AARDtj6wL3U
