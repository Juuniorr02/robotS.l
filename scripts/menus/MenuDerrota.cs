using Godot;

public partial class MenuDerrota : CanvasLayer
{
    private Button btnVolver;
    private Button btnReiniciar;
	private int healthActual;
    private bool isPaused = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        btnVolver = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/botones/volver");
        btnReiniciar = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/botones/reiniciar");

        ConfigurarBoton(btnVolver);
        ConfigurarBoton(btnReiniciar);

        if (btnReiniciar != null)
            btnReiniciar.Pressed += OnReiniciar;

        if (btnVolver != null)
            btnVolver.Pressed += OnGuardarSalir;

        Visible = false;
    }

	public override void _Process(double delta)
    {
        UpdateMenuDerrota();
    }

    private void ConfigurarBoton(Button b)
    {
        if (b == null) return;

        b.ProcessMode = ProcessModeEnum.Always;
        b.MouseFilter = Control.MouseFilterEnum.Stop;
    }

    public void UpdateMenuDerrota()
	{
		healthActual = Recursos.Instance.Health;

		if (Recursos.Instance.Health <= 0)
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
        Recursos.Instance.ReiniciarEnergia();
    	GD.Print("Reiniciar partida");
    	GetTree().ReloadCurrentScene();
	}

	private void OnGuardarSalir()
	{
    	QuitarPausa();
    	Input.MouseMode = Input.MouseModeEnum.Visible;
    	GetTree().ChangeSceneToFile("res://scenes/menus/menu.tscn");
	}
}