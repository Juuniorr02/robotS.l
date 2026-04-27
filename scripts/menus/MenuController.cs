using Godot;
using System;
using System.Collections;

public partial class MenuController : Node
{
    private PackedScene optionsScene;

    public override void _Ready()
    {
        Node menuRoot = GetParent();

        Button newBtn = FindButton(menuRoot, "Nueva partida", "NewGame");
        Button optionsBtn = FindButton(menuRoot, "Opciones", "Options");
        Button quitBtn = FindButton(menuRoot, "Salir", "Quit");

        if (newBtn != null)
            newBtn.Pressed += OnNewGame;

        if (optionsBtn != null)
            optionsBtn.Pressed += OnOptions;

        if (quitBtn != null)
            quitBtn.Pressed += OnQuit;

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private Button FindButton(Node root, string textMatch, string nameContains)
    {
        if (root == null) return null;

        foreach (var c in root.GetChildren())
        {
            if (c is Button b)
            {
                if (b.Text == textMatch || b.Name.ToString().Contains(nameContains))
                    return b;
            }

            if (c is Node n)
            {
                var found = FindButton(n, textMatch, nameContains);
                if (found != null) return found;
            }
        }

        return null;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
                GetTree().Quit();
        }
    }

    public void OnNewGame()
    {
        Recursos.Instance.ReiniciarEnergia();
        GetTree().ChangeSceneToFile("res://scenes/niveles/level1.tscn");
    }

    public void OnOptions()
    {
        GetTree().ChangeSceneToFile("res://scenes/menus/options.tscn");
    }

    public void OnQuit()
    {
        GetTree().Quit();
    }
}