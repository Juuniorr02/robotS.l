using Godot;
using System;
using GameConstants; // Asegúrate de tener este namespace para TipoTropa

public partial class Iu : Control
{
    private Button MejorarButton;
    private Label EnergiaLabel;
    private Label VidaEnemigoLabel;
    private Label HealthLabel;

    public TextureButton Ligero;
    public TextureButton Penetrador;
    public TextureButton Tanque;
    public TextureButton Artillero;

    // --- NUEVA VARIABLE ---
    [Export] private Spawner _spawnerJugador; 

    private int contador = 0;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        
        MejorarButton = GetNode<Button>("%MejorarButton");
        EnergiaLabel = GetNode<Label>("%EnergiaLabel");
        VidaEnemigoLabel = GetNode<Label>("%VidaEnemigoLabel");
        HealthLabel = GetNode<Label>("%HealthLabel");

        Ligero = GetNodeOrNull<TextureButton>("%Ligero");
        Penetrador = GetNodeOrNull<TextureButton>("%Penetrador");
        Tanque = GetNodeOrNull<TextureButton>("%Tanque");
        Artillero = GetNodeOrNull<TextureButton>("%Artillero");

        // --- CONEXIÓN DE BOTONES DE TROPAS ---
        if (Ligero != null) Ligero.Pressed += () => OnTropaButtonPressed(TipoTropa.Ligero, 100);
        if (Penetrador != null) Penetrador.Pressed += () => OnTropaButtonPressed(TipoTropa.Penetrador, 125);
        if (Tanque != null) Tanque.Pressed += () => OnTropaButtonPressed(TipoTropa.Tanque, 150);
        if (Artillero != null) Artillero.Pressed += () => OnTropaButtonPressed(TipoTropa.Artillero, 200);

        MejorarButton.Text = $"Mejorar: ({100} energia)";

        ConfigurarBoton(Ligero);
        ConfigurarBoton(Penetrador);
        ConfigurarBoton(Tanque);
        ConfigurarBoton(Artillero);

        if (MejorarButton != null)
            MejorarButton.Pressed += OnMejorarButtonPressed;

        ActualizarIUCadaSegundo();
    }

    // --- NUEVA FUNCIÓN PARA SPAWNEAR ---
    private void OnTropaButtonPressed(TipoTropa tipo, int coste)
    {
        if (Recursos.Instance.Energia >= coste)
        {
            if (_spawnerJugador != null)
            {
                Recursos.Instance.Energia -= coste;
                _spawnerJugador.Spawn(tipo);
                UpdateIU(); // Actualiza el texto inmediatamente
            }
            else
            {
                GD.PrintErr("Error: No has asignado el SpawnerJugador en el Inspector de la IU");
            }
        }
        else
        {
            GD.Print("Energía insuficiente para " + tipo);
        }
    }
    
    private void ConfigurarBoton(TextureButton b)
    {
        if (b == null) return;
        b.ProcessMode = ProcessModeEnum.Pausable;
        b.MouseFilter = MouseFilterEnum.Stop;
    }

    private async void ActualizarIUCadaSegundo()
    {
        while (true)
        {
            UpdateIU();
            await ToSignal(GetTree().CreateTimer(0.1f, false), "timeout");
        }
    }

    private void UpdateIU()
    {
        Recursos.Instance.SubidaEnergia();
        EnergiaLabel.Text = $"Energia: {(int)Recursos.Instance.Energia}";
        HealthLabel.Text = $"Vida: {(int)Recursos.Instance.Health}";
        VidaEnemigoLabel.Text = $"Vida Enemigo: {(int)Recursos.Instance.VidaEnemigo}";
    }

    private void OnMejorarButtonPressed()
    {
        // ... (Tu código de OnMejorarButtonPressed se mantiene igual) ...
        int coste = 100 + (contador * 100);

        if (contador < 4 && Recursos.Instance.Energia >= coste)
        {
            Recursos.Instance.Energia -= coste;
            Recursos.Instance.MejorarEnergia();
            contador++;
            
            if (contador < 4)
                MejorarButton.Text = $"Mejorar: ({coste + 100} energia)";
            else
            {
                MejorarButton.Text = $"Mejorar: (MAX)";
                MejorarButton.Disabled = true;
            }
            UpdateIU();
        }
    }
}
