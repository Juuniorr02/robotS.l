using Godot;
using System;

public partial class Iu : Control
{
    private Button MejorarButton;
    private Label EnergiaLabel;

    public TextureButton Ligero;
    public TextureButton Penetrador;
    public TextureButton Tanque;
    public TextureButton Artillero;

    private int contador = 0;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        
        MejorarButton = GetNode<Button>("%MejorarButton");
        EnergiaLabel = GetNode<Label>("%EnergiaLabel");

        Ligero = GetNodeOrNull<TextureButton>("%Ligero");
        Penetrador = GetNodeOrNull<TextureButton>("%Penetrador");
        Tanque = GetNodeOrNull<TextureButton>("%Tanque");
        Artillero = GetNodeOrNull<TextureButton>("%Artillero");

        MejorarButton.Text = $"Mejorar: ({100} energia)";

        ConfigurarBoton(Ligero);
        ConfigurarBoton(Penetrador);
        ConfigurarBoton(Tanque);
        ConfigurarBoton(Artillero);

        if (MejorarButton != null)
            MejorarButton.Pressed += OnMejorarButtonPressed;

        ActualizarIUCadaSegundo();
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
            await ToSignal(GetTree().CreateTimer(1.0f, false), "timeout");
        }
    }

    private void UpdateIU()
    {
        Recursos.Instance.SubidaEnergia();
        EnergiaLabel.Text = $"Energia: {Recursos.Instance.Energia}";
    }

    private void OnMejorarButtonPressed()
    {
        int coste = 100;

        if (contador == 0 && Recursos.Instance.Energia >= 100)
        {
            coste = 200;
            MejorarButton.Text = $"Mejorar: ({coste} energia)";
            Recursos.Instance.Energia -= 100;
            Recursos.Instance.MejorarEnergia();
            UpdateIU();
            contador++;
        }
        else if (contador == 1 && Recursos.Instance.Energia >= 200)
        {
            coste = 300;
            MejorarButton.Text = $"Mejorar: ({coste} energia)";
            Recursos.Instance.Energia -= 200;
            Recursos.Instance.MejorarEnergia();
            UpdateIU();
            contador++;
        }
        else if (contador == 2 && Recursos.Instance.Energia >= 300)
        {
            coste = 400;
            MejorarButton.Text = $"Mejorar: ({coste} energia)";
            Recursos.Instance.Energia -= 300;
            Recursos.Instance.MejorarEnergia();
            UpdateIU();
            contador++;
        }

        else if (contador == 3 && Recursos.Instance.Energia >= 400)
        {
            Recursos.Instance.Energia -= 400;
            Recursos.Instance.MejorarEnergia();
            UpdateIU();
            contador++;
        }
        else if (contador == 4)
        {
            MejorarButton.Text = $"Mejorar: (MAX)";
            MejorarButton.Disabled = true;
        }
    }
}
