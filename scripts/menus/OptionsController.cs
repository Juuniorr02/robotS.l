using Godot;
using System;
using System.IO;

public partial class OptionsController : Control
{
    private OptionButton resolucionOption;
    private OptionButton modoOption;
    private OptionButton calidadOption;
    private HSlider volumeOption;
    private HSlider sfxOption;
    private HSlider musicaOption;
    private SpinBox sensibilidadOption;
    private CheckBox invertirYOption;

    private const string ConfigPath = "user://config.cfg";
    
    // Rutas base
    private const string BasePath = "CanvasLayer/CenterContainer/PanelContainer/MarginContainer/PanelContainer/VBoxContainer/VBoxContainer";
    private const string ResolucionPath = BasePath + "/Resolucion/Resolucion/Resolucion/ResolucionOption";
    private const string ModoPath = BasePath + "/Modo/Modo/Modo/ModoOption";
    private const string CalidadPath = BasePath + "/Calidad/Calidad/Calidad/CalidadOption";
    private const string VolumenPath = BasePath + "/Volumen/Volumen/Volumen/VolumenOption";
    private const string SFXPath = BasePath + "/SFX/SFX/SFX/SFXOption";
    private const string MusicaPath = BasePath + "/Musica/Musica/Musica/MusicaOption";
    private const string ApplyButtonPath = BasePath + "/Botones/Botones/Aplicar";
    private const string ExitButtonPath = BasePath + "/Botones/Botones/Salir";
    private const string BackButtonPath = BasePath + "/Botones/Botones/Volver";
    private const string SensibilidadPath = BasePath + "/Sensibilidad/Sensibilidad/Sensibilidad/SensibilidadOption";
    private const string InvertirYPath = BasePath + "/InvertirY/InvertirY/InvertirY/InvertirYOption";

    public override void _Ready()
    {
        resolucionOption = GetNode<OptionButton>(ResolucionPath);
        modoOption = GetNode<OptionButton>(ModoPath);
        calidadOption = GetNode<OptionButton>(CalidadPath);
        volumeOption = GetNode<HSlider>(VolumenPath);
        sfxOption = GetNode<HSlider>(SFXPath);
        musicaOption = GetNode<HSlider>(MusicaPath);
        sensibilidadOption = GetNodeOrNull<SpinBox>(SensibilidadPath);
        invertirYOption = GetNodeOrNull<CheckBox>(InvertirYPath);
        
        if (invertirYOption == null)
        {
            GD.Print($"_Ready: CheckBox no encontrado en ruta: {InvertirYPath}");
            GD.Print($"BasePath: {BasePath}");
            // Listar todos los hijos para debug
            var children = GetNode(BasePath).GetChildren();
            GD.Print($"Hijos de BasePath: {children.Count}");
            foreach (var child in children)
            {
                GD.Print($"  - {child.Name} ({child.GetType().Name})");
            }
        }
        else
        {
            GD.Print("_Ready: CheckBox invertirYOption encontrado correctamente");
        }

        // Añadir resoluciones disponibles
        resolucionOption.AddItem("800x600");
        resolucionOption.AddItem("1280x720");
        resolucionOption.AddItem("1920x1080");
        resolucionOption.AddItem("2560x1440");
        resolucionOption.AddItem("3840x2160");

        // Añadir modos de pantalla
        modoOption.AddItem("Ventana");
        modoOption.AddItem("Pantalla completa");
        modoOption.AddItem("Sin bordes");

        calidadOption.AddItem("Baja");
        calidadOption.AddItem("Media");
        calidadOption.AddItem("Alta");

        // Conectar botones
        GetNode<Button>(ApplyButtonPath).Pressed += OnApply;
        GetNode<Button>(ExitButtonPath).Pressed += OnExit;
        GetNode<Button>(BackButtonPath).Pressed += OnBack;

        // Cargar configuración si existe
        LoadConfig();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                GetTree().ChangeSceneToFile("res://scenes/menus/Menu.tscn");
            }
        }
    }

    private void OnApply()
    {
        // Cambiar resolución
        string res = resolucionOption.GetItemText(resolucionOption.Selected);
        string[] parts = res.Split('x');
        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);

        // Cambiar modo de pantalla
        string mode = modoOption.GetItemText(modoOption.Selected);
        DisplayServer.WindowMode windowMode = DisplayServer.WindowMode.Windowed;

        if (mode == "Pantalla completa")
            windowMode = DisplayServer.WindowMode.Fullscreen;
        else if (mode == "Sin bordes")
            windowMode = DisplayServer.WindowMode.ExclusiveFullscreen;

        DisplayServer.WindowSetMode(windowMode);
        DisplayServer.WindowSetSize(new Vector2I(width, height));

        // Obtener calidad
        string calidad = calidadOption.GetItemText(calidadOption.Selected);

        // Cambiar volumen general, SFX y música
        float volume = (float)volumeOption.Value;
        float sfx = (float)sfxOption.Value;
        float musica = (float)musicaOption.Value;
        
        int masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0)
            AudioServer.SetBusVolumeDb(masterBus, Linear2Db(volume));
        
        int sfxBus = AudioServer.GetBusIndex("SFX");
        if (sfxBus >= 0)
            AudioServer.SetBusVolumeDb(sfxBus, Linear2Db(sfx));
        
        int musicaBus = AudioServer.GetBusIndex("Musica");
        if (musicaBus >= 0)
            AudioServer.SetBusVolumeDb(musicaBus, Linear2Db(musica));

        // Obtener sensibilidad e invertir
        float sensibilidad = 1f;
        bool invertirY = false;
        
        if (sensibilidadOption != null)
            sensibilidad = (float)sensibilidadOption.Value;
        
        if (invertirYOption != null)
        {
            invertirY = invertirYOption.ButtonPressed;
            GD.Print($"OnApply: invertirY encontrado, ButtonPressed={invertirY}");
        }
        else
        {
            GD.Print("OnApply: invertirY NO ENCONTRADO (es null)");
        }

        // Guardar configuración
        SaveConfig(width, height, mode, calidad, volume, sfx, musica, sensibilidad, invertirY);
    }

    private void OnExit()
    {
        string res = resolucionOption.GetItemText(resolucionOption.Selected);
        string[] parts = res.Split('x');
        int width = int.Parse(parts[0]);
        int height = int.Parse(parts[1]);

        // Cambiar modo de pantalla
        string mode = modoOption.GetItemText(modoOption.Selected);
        DisplayServer.WindowMode windowMode = DisplayServer.WindowMode.Windowed;

        if (mode == "Pantalla completa")
            windowMode = DisplayServer.WindowMode.Fullscreen;
        else if (mode == "Sin bordes")
            windowMode = DisplayServer.WindowMode.ExclusiveFullscreen;

        DisplayServer.WindowSetMode(windowMode);
        DisplayServer.WindowSetSize(new Vector2I(width, height));

        // Obtener calidad
        string calidad = calidadOption.GetItemText(calidadOption.Selected);

        // Cambiar volumen general, SFX y música
        float volume = (float)volumeOption.Value;
        float sfx = (float)sfxOption.Value;
        float musica = (float)musicaOption.Value;
        
        int masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0)
            AudioServer.SetBusVolumeDb(masterBus, Linear2Db(volume));
        
        int sfxBus = AudioServer.GetBusIndex("SFX");
        if (sfxBus >= 0)
            AudioServer.SetBusVolumeDb(sfxBus, Linear2Db(sfx));
        
        int musicaBus = AudioServer.GetBusIndex("Musica");
        if (musicaBus >= 0)
            AudioServer.SetBusVolumeDb(musicaBus, Linear2Db(musica));

        // Obtener sensibilidad e invertir
        float sensibilidad = 5f;
        bool invertirY = false;
        
        if (sensibilidadOption != null)
            sensibilidad = (float)sensibilidadOption.Value;
        
        if (invertirYOption != null)
        {
            invertirY = invertirYOption.ButtonPressed;
            GD.Print($"OnExit: invertirY encontrado, ButtonPressed={invertirY}");
        }
        else
        {
            GD.Print("OnExit: invertirY NO ENCONTRADO (es null)");
        }

        // Guardar configuración
        SaveConfig(width, height, mode, calidad, volume, sfx, musica, sensibilidad, invertirY);
        GetTree().ChangeSceneToFile("res://scenes/ui/Menu.tscn");
    }

    private void OnBack()
    {
        GetTree().ChangeSceneToFile("res://scenes/menus/Menu.tscn");
    }

    // Función auxiliar para convertir de 0..1 a decibelios
    private float Linear2Db(float linear)
    {
        if (linear <= 0) return -80;
        return 20f * (Mathf.Log(linear) / Mathf.Log(10f));
    }

    // Guarda la configuración en un archivo
	private void SaveConfig(int width, int height, string mode, string calidad, float volume, float sfx, float musica, float sensibilidad, bool invertirY)
	{
    	var config = new ConfigFile();

    	// Usar ruta user://
    	const string path = "user://config.cfg";

    	// Cargar si existe
    	Error err = config.Load(path);
    	if (err != Error.Ok)
        	config = new ConfigFile(); // archivo nuevo

    	// Guardar valores
    	config.SetValue("display", "width", width);
    	config.SetValue("display", "height", height);
    	config.SetValue("display", "mode", mode);
    	config.SetValue("display", "calidad", calidad);
    	config.SetValue("audio", "volume", volume);
    	config.SetValue("audio", "sfx", sfx);
    	config.SetValue("audio", "musica", musica);
    	config.SetValue("gameplay", "sensibilidad", sensibilidad);
    	config.SetValue("gameplay", "invertirY", invertirY ? "true" : "false");

    	// Guardar archivo
    	config.Save(path);
    	GD.Print($"SaveConfig: invertirY guardado como '{(invertirY ? "true" : "false")}'");
	}

    // Carga la configuración desde el archivo si existe
    public void LoadConfig()
    {
        var config = new ConfigFile();
        if (config.Load(ConfigPath) != Error.Ok)
            return; // no existe el archivo, usar valores por defecto

        int width = (int)config.GetValue("display", "width", 1920);
        int height = (int)config.GetValue("display", "height", 1080);
        string mode = (string)config.GetValue("display", "mode", "Ventana");
        string calidad = (string)config.GetValue("display", "calidad", "Media");
        float volumen = (float)config.GetValue("audio", "volume", 1f);
        float sfx = (float)config.GetValue("audio", "sfx", 1f);
        float musica = (float)config.GetValue("audio", "musica", 1f);
        float sensibilidad = (float)config.GetValue("gameplay", "sensibilidad", 1f);
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

        int masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0)
            AudioServer.SetBusVolumeDb(masterBus, Linear2Db(volumen));
        
        int sfxBus = AudioServer.GetBusIndex("SFX");
        if (sfxBus >= 0)
            AudioServer.SetBusVolumeDb(sfxBus, Linear2Db(sfx));
        
        int musicaBus = AudioServer.GetBusIndex("Musica");
        if (musicaBus >= 0)
            AudioServer.SetBusVolumeDb(musicaBus, Linear2Db(musica));

        // Reflejar en UI
        string resText = $"{width}x{height}";
        for (int i = 0; i < resolucionOption.GetItemCount(); i++)
        {
            if (resolucionOption.GetItemText(i) == resText)
            {
                resolucionOption.Selected = i;
                break;
            }
        }

        for (int i = 0; i < modoOption.GetItemCount(); i++)
        {
            if (modoOption.GetItemText(i) == mode)
            {
                modoOption.Selected = i;
                break;
            }
        }

        for (int i = 0; i < calidadOption.GetItemCount(); i++)
        {
            if (calidadOption.GetItemText(i) == calidad)
            {
                calidadOption.Selected = i;
                break;
            }
        }

        volumeOption.Value = volumen;
        sfxOption.Value = sfx;
        musicaOption.Value = musica;
        
        if (sensibilidadOption != null)
            sensibilidadOption.Value = sensibilidad;
        if (invertirYOption != null)
            invertirYOption.ButtonPressed = invertirY;
    }
}