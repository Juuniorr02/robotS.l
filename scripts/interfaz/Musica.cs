using Godot;
using System;

public partial class Musica : Node
{
    private AudioStreamPlayer musicPlayer;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // 🔥 importante (no se pausa)

        musicPlayer = new AudioStreamPlayer();
        AddChild(musicPlayer);

        var music = GD.Load<AudioStream>("res://assets/musica/musica.mp3"); // tu ruta
        musicPlayer.Stream = music;
        musicPlayer.VolumeDb = -10; // opcional
        musicPlayer.Autoplay = true;

        musicPlayer.Play();
    }
}