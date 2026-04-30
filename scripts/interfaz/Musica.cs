using Godot;
using System;

public partial class Musica : Node
{
    private AudioStreamPlayer musicPlayer;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        musicPlayer = new AudioStreamPlayer();
        AddChild(musicPlayer);

        var music = GD.Load<AudioStream>("res://assets/musica/musica.mp3");

        if (music is AudioStreamMP3 mp3)
        {
            mp3.Loop = true;
        }

        musicPlayer.Stream = music;
        musicPlayer.VolumeDb = -10;
        musicPlayer.Autoplay = true;

        musicPlayer.Play();
    }
}