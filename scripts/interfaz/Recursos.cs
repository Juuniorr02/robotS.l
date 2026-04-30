using Godot;
using System;

public partial class Recursos : Node
{
	public static Recursos Instance;
	private const string ConfigPath = "user://config.cfg";

	public int Energia = 0;
	public int EnergiaMejorada = 0;
	public int Health = 1000;
	public int VidaEnemigo = 2500;

	public override void _Ready()
	{
		Instance = this;
		SubidaEnergia();
	}

	public void SubidaEnergia()
	{
		SumarEnergia();
	}

	public void SumarEnergia()
	{
		Energia += 1 + EnergiaMejorada;
	}

	public void MejorarEnergia()
	{
		EnergiaMejorada += 1;
	}

    public void DanarJugador(int cantidad)
    {
        Health -= cantidad;
        if (Health < 0) Health = 0;
    }

    public void DanarEnemigo(int cantidad)
    {
        VidaEnemigo -= cantidad;
        if (VidaEnemigo < 0) VidaEnemigo = 0;
    }

	public void ReiniciarEnergia()
	{
		Energia = 0;
		EnergiaMejorada = 0;
		Health = 1000;
		VidaEnemigo = 2500;
	}

	public void LoadConfig()
    {
        var config = new ConfigFile();
        if (config.Load(ConfigPath) != Error.Ok)
            return; // no existe el archivo, usar valores por defecto

        int width = (int)config.GetValue("display", "width", 1920);
        int height = (int)config.GetValue("display", "height", 1080);
        string mode = (string)config.GetValue("display", "mode", "Ventana");
        string invertirYStr = (string)config.GetValue("gameplay", "invertirY", "false");
        bool invertirY = invertirYStr == "true";
        
        GD.Print($"LoadConfig: invertirY cargado como '{invertirYStr}' (bool: {invertirY})");

        // Aplicar configuración
        DisplayServer.WindowSetSize(new Vector2I(width, height));
        DisplayServer.WindowSetMode(mode switch
        {
            "Pantalla completa" => DisplayServer.WindowMode.Fullscreen,
            "Sin bordes" => DisplayServer.WindowMode.ExclusiveFullscreen,
            _ => DisplayServer.WindowMode.Windowed
        });
	}
}
