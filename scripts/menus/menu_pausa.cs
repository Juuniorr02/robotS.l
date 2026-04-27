using Godot;

public partial class menu_pausa : CanvasLayer
{
    private Button btnVolver;
    private Button btnReiniciar;
    private Button btnSalir;

    private float lastPressTime = -1f;
    private float doublePressThreshold = 0.3f;

    private bool isPaused = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        btnVolver = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/volver");
        btnReiniciar = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/reiniciar");
        btnSalir = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/salir");

        ConfigurarBoton(btnVolver);
        ConfigurarBoton(btnReiniciar);
        ConfigurarBoton(btnSalir);
            
        if (btnReiniciar != null)
            btnReiniciar.Pressed += OnReiniciar;

        if (btnSalir != null)
            btnSalir.Pressed += OnSalir;

        if (btnVolver != null)
            btnVolver.Pressed += Salir;

        Visible = false;
    }

    private void ConfigurarBoton(Button b)
    {
        if (b == null) return;

        b.ProcessMode = ProcessModeEnum.Always;
        b.MouseFilter = Control.MouseFilterEnum.Stop;
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("pausa"))
        {
            float currentTime = Time.GetTicksMsec() / 2000.0f;

            if (lastPressTime > 0 && currentTime - lastPressTime <= doublePressThreshold)
            {
                if (isPaused) QuitarPausa();
                else Pausar();

                lastPressTime = -1f;
            }

            else
            {
            lastPressTime = currentTime;
            }
        }
    }

    public void Pausar()
    {
        isPaused = true;
        GetTree().Paused = true;
        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

	private void Salir()
    {
        QuitarPausa();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile("res://scenes/ui/menus/menu.tscn");
    }

    public void QuitarPausa()
    {
        isPaused = false;
        GetTree().Paused = false;
        Visible = false;
    }

    private void OnReiniciar()
	{
		QuitarPausa();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Recursos.Instance.ReiniciarEnergia();
    	GD.Print("Reiniciar partida");
    	GetTree().ReloadCurrentScene();
	}

    private void OnSalir()
    {
        QuitarPausa();
    }
}