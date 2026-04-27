using Godot;
using System;
using System.Linq;
using GameConstants;
using System.Collections.Generic;

public partial class BaseEnemigo : Node2D
{
    [Export] public PackedScene EscenaLigero, EscenaTanque, EscenaPenetrador, EscenaArtillero;
	
    
    // Economía de la IA
    private float _energiaEnemiga = 0;
    [Export] public float VelocidadCargaEnergia = 10f;

    // Costes de cada unidad
    private readonly Dictionary<TipoTropa, int> _costes = new() {
        { TipoTropa.Ligero, 50 },
        { TipoTropa.Tanque, 150 },
        { TipoTropa.Penetrador, 100 },
        { TipoTropa.Artillero, 120 }
    };

    private Node _unitsContainer;

    public override void _Ready() => _unitsContainer = GetParent().GetNode("UnitsContainer");

    public override void _Process(double delta)
    {
        _energiaEnemiga += VelocidadCargaEnergia * (float)delta;
        IntentarReaccionar();
    }

    private void IntentarReaccionar()
    {
        // 1. Analizar la amenaza más cercana a la base enemiga
        TipoTropa? amenaza = DetectarAmenazaPrincipal();

        if (amenaza.HasValue)
        {
            // 2. Elegir el "counter" ideal
            TipoTropa respuesta = ObtenerCounter(amenaza.Value);

            // 3. Si hay dinero, spawnear
            if (_energiaEnemiga >= _costes[respuesta])
            {
                Spawnear(respuesta);
                _energiaEnemiga -= _costes[respuesta];
            }
        }
        else if (_energiaEnemiga >= 200) // Si no hay amenazas, spawnear algo básico para presionar
        {
            Spawnear(TipoTropa.Ligero);
            _energiaEnemiga -= _costes[TipoTropa.Ligero];
        }
    }

    private TipoTropa? DetectarAmenazaPrincipal()
    {
        // Buscamos en el UnitsContainer tropas que pertenezcan al jugador
        // Asumiendo que tus tropas tienen una propiedad 'EsDelJugador' y 'Tipo'
        var tropasJugador = _unitsContainer.GetChildren()
            .OfType<Robot>() // Clase base de tus tropas
            .Where(t => t.EsDelJugador)
            .OrderBy(t => t.GlobalPosition.DistanceTo(this.GlobalPosition));

        return tropasJugador.FirstOrDefault()?.Tipo;
    }

    private TipoTropa ObtenerCounter(TipoTropa amenaza)
    {
        return amenaza switch
        {
            TipoTropa.Artillero => TipoTropa.Ligero,     // Ligero gana a Artillero
            TipoTropa.Penetrador => TipoTropa.Artillero, // Artillero gana a Penetrador
            TipoTropa.Tanque => TipoTropa.Penetrador,    // Penetrador gana a Tanque
            TipoTropa.Ligero => TipoTropa.Tanque,        // Tanque gana a Ligero
            _ => TipoTropa.Ligero
        };
    }

    private void Spawnear(TipoTropa tipo)
    {
        PackedScene escena = tipo switch {
            TipoTropa.Ligero => EscenaLigero,
            TipoTropa.Tanque => EscenaTanque,
            TipoTropa.Penetrador => EscenaPenetrador,
            _ => EscenaArtillero
        };

        var instancia = escena.Instantiate<Robot>();
        _unitsContainer.AddChild(instancia);
        instancia.GlobalPosition = GetNode<Marker2D>("Marker2D").GlobalPosition;
    }
}
