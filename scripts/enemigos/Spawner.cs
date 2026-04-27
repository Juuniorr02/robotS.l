using Godot;
using System;
using GameConstants;

public partial class Spawner : Marker2D
{
    [Export] public PackedScene EscenaLigero, EscenaTanque, EscenaPenetrador, EscenaArtillero;
    [Export] public bool EsSpawnerJugador = true;

    private Node _unitsContainer;

    public override void _Ready()
    {
        // Buscamos el contenedor de unidades (ajusta la ruta según tu escena)
        _unitsContainer = GetParent().GetNode<Node>("../UnitsContainer");
    }

    public void Spawn(TipoTropa tipo)
    {
        PackedScene escena = tipo switch
        {
            TipoTropa.Ligero => EscenaLigero,
            TipoTropa.Tanque => EscenaTanque,
            TipoTropa.Penetrador => EscenaPenetrador,
            TipoTropa.Artillero => EscenaArtillero,
            _ => null
        };

        if (escena != null)
        {
            Robot instancia = escena.Instantiate<Robot>();
            
            // Configuramos la tropa antes de añadirla al árbol
            instancia.EsDelJugador = EsSpawnerJugador;
            instancia.Tipo = tipo;
            
            _unitsContainer.AddChild(instancia);
            instancia.GlobalPosition = this.GlobalPosition;
        }
    }
}
