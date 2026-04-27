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
        MejorarButton = GetNode<Button>("%MejorarButton");
        EnergiaLabel = GetNode<Label>("%EnergiaLabel");

        Ligero = GetNodeOrNull<TextureButton>("%Ligero");
        Penetrador = GetNodeOrNull<TextureButton>("%Penetrador");
        Tanque = GetNodeOrNull<TextureButton>("%Tanque");
        Artillero = GetNodeOrNull<TextureButton>("%Artillero");

        ConfigurarBoton(Ligero);
        ConfigurarBoton(Penetrador);
        ConfigurarBoton(Tanque);
        ConfigurarBoton(Artillero);

        if (MejorarButton != null)
            MejorarButton.Pressed += OnMejorarButtonPressed;

        UpdateIU();
    }
    
    private void ConfigurarBoton(TextureButton b)
    {
        if (b == null) return;

        b.ProcessMode = ProcessModeEnum.Always;
        b.MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Process(double delta)
    {
        UpdateIU();
    }

    private void UpdateIU()
    {
        EnergiaLabel.Text = $"Energia: {Recursos.Instance.Energia}";
    }

    private void OnMejorarButtonPressed()
    {
        if (contador < 5)
        {
            Recursos.Instance.MejorarEnergia();
            contador++;
        }
    }
}
