using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    // Definimos los tipos de robots
    public enum RobotType { Ligero, Tanque, Penetrador, Artillero }

    // Recursos de escenas (Debes asignar los .tscn en el inspector)
    [Export] public PackedScene RobotLigeroScene;
    [Export] public PackedScene RobotTanqueScene;
    [Export] public PackedScene RobotPenetradorScene;
	[Export] public PackedScene RobotArtilleroScene;

    // Economía
    public float Energia { get; private set; } = 0;
    [Export] public float EnergiaPorSegundo = 10f;
    
    // Costes
    private Dictionary<RobotType, int> _costes = new Dictionary<RobotType, int>
    {
        { RobotType.Ligero, 50 },
        { RobotType.Tanque, 150 },
        { RobotType.Penetrador, 100 },
        { RobotType.Artillero, 200 }
    };

    private Marker2D _spawnPoint;
    private Node2D _unitsContainer;
    private Label _energiaLabel; // Para mostrar el dinero en pantalla

    public override void _Ready()
    {
        _spawnPoint = GetNode<Marker2D>("../Battlefield/BaseJugador/Marker2D");
        _unitsContainer = GetNode<Node2D>("../Battlefield/UnitsContainer");
        _energiaLabel = GetNode<Label>("../UI/EnergiaLabel"); // Asegúrate de crear este Label
    }

    public override void _Process(double delta)
    {
        // Generación pasiva de energía
        Energia += EnergiaPorSegundo * (float)delta;
        _energiaLabel.Text = $"Energía: {(int)Energia}";
    }

    // Este método lo conectarás a las señales "pressed" de tus botones
    public void OnRobotButtonPressed(string typeName)
    {
        if (Enum.TryParse(typeName, out RobotType selectedType))
        {
            int coste = _costes[selectedType];

            if (Energia >= coste)
            {
                Energia -= coste;
                SpawnRobot(selectedType);
            }
            else
            {
                GD.Print("¡No hay suficiente energía!");
            }
        }
    }

    private void SpawnRobot(RobotType type)
    {
        PackedScene sceneToSpawn = type switch
        {
            RobotType.Ligero => RobotLigeroScene,
            RobotType.Tanque => RobotTanqueScene,
            RobotType.Penetrador => RobotPenetradorScene,
			RobotType.Artillero => RobotArtilleroScene,
            _ => null
        };

        if (sceneToSpawn != null)
        {
            var instance = sceneToSpawn.Instantiate<Robot>(); // Tu clase base Robot
            _unitsContainer.AddChild(instance);
            instance.GlobalPosition = _spawnPoint.GlobalPosition;
        }
    }
}
