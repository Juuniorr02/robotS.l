using Godot;

public partial class MenuVictoria : CanvasLayer
{
    private Button btnSiguiente;
    private Button btnVolver;
    private Button btnReiniciar;

    private bool isPaused = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        btnSiguiente = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/botones/siguiente");
        btnVolver = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/botones/volver");
        btnReiniciar = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/botones/reiniciar");

        ConfigurarBoton(btnSiguiente);
        ConfigurarBoton(btnVolver);
        ConfigurarBoton(btnReiniciar);

        if (btnSiguiente != null)
            btnSiguiente.Pressed += OnSiguiente;

        if (btnReiniciar != null)
            btnReiniciar.Pressed += OnReiniciar;

        if (btnVolver != null)
            btnVolver.Pressed += OnGuardarSalir;

        Visible = false;
    }

	public override void _Process(double delta)
    {
        UpdateMenuVictoria();
    }

    private void ConfigurarBoton(Button b)
    {
        if (b == null) return;

        b.ProcessMode = ProcessModeEnum.Always;
        b.MouseFilter = Control.MouseFilterEnum.Stop;
    }

    public void UpdateMenuVictoria()
    {
        if (Recursos.Instance.VidaEnemigo <= 0)
        {
           Pausar(); 
        }
    }

    private void Pausar()
    {
        isPaused = true;
        GetTree().Paused = true;
        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void QuitarPausa()
    {
        isPaused = false;
        GetTree().Paused = false;
        Visible = false;
    }

    private void OnReiniciar()
	{
		QuitarPausa();
        Recursos.Instance.ReiniciarEnergia();;
    	GD.Print("Reiniciar partida");
    	GetTree().ReloadCurrentScene();
	}

	private void OnGuardarSalir()
	{
		QuitarPausa();
    	GD.Print("Guardar y salir");

    	QuitarPausa();
    	Input.MouseMode = Input.MouseModeEnum.Visible;
    	GetTree().ChangeSceneToFile("res://scenes/menus/Menu.tscn");
	}

	private void OnSiguiente()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
        QuitarPausa();
    	GetTree().ChangeSceneToFile("res://scenes/menus/Menu.tscn");
	}
}