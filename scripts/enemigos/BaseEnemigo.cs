using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameConstants;

public partial class BaseEnemigo : Node2D
{
    // Referencia al script modular de Spawn
    private Spawner _spawner;
    private Node _unitsContainer;

    // Economía interna de la IA (Espejo del jugador)
    private float _energiaIA = 0;
    private float _energiaPorSegundoBase = 10f;
    private int _nivelEconomia = 0;
    private const int MaxNivelEconomia = 4;

    // Diccionario de costes (Idénticos a los del jugador)
    private readonly Dictionary<TipoTropa, int> _costes = new() {
        { TipoTropa.Ligero, 50 },
        { TipoTropa.Tanque, 150 },
        { TipoTropa.Penetrador, 100 },
        { TipoTropa.Artillero, 200 }
    };

    public override void _Ready()
    {
        // Buscamos el spawner que creamos antes (ajusta el nombre si es diferente)
        _spawner = GetNode<Spawner>("SpawnerEnemigo"); 
        _unitsContainer = GetParent().GetNode("UnitsContainer");
    }

    public override void _Process(double delta)
    {
        // 1. Generar energía según el nivel de economía
        float multiplicador = 1.0f + (_nivelEconomia * 0.5f);
        _energiaIA += _energiaPorSegundoBase * multiplicador * (float)delta;

        // 2. Ejecutar la lógica de decisión
        PensarJugada();
    }

    private void PensarJugada()
    {
        Robot amenaza = DetectarAmenazaMasCercana();

        if (amenaza != null)
        {
            // --- CASO A: HAY ENEMIGOS ---
            TipoTropa respuesta = ObtenerCounterIdeal(amenaza.Tipo);
            
            if (_energiaIA >= _costes[respuesta])
            {
                _energiaIA -= _costes[respuesta];
                _spawner.Spawn(respuesta);
            }
        }
        else 
        {
            // --- CASO B: EL CAMPO ESTÁ LIBRE ---
            int costeMejora = 100 + (_nivelEconomia * 100);

            // Si puede mejorar la economía, lo hace primero
            if (_nivelEconomia < MaxNivelEconomia && _energiaIA >= costeMejora)
            {
                _energiaIA -= costeMejora;
                _nivelEconomia++;
                GD.Print($"[IA] Economía mejorada a Nivel {_nivelEconomia}");
            }
            // Si ya está mejorado o tiene energía de sobra, presiona con tropas ligeras
            else if (_energiaIA >= 250)
            {
                _energiaIA -= _costes[TipoTropa.Ligero];
                _spawner.Spawn(TipoTropa.Ligero);
            }
        }
    }

    private Robot DetectarAmenazaMasCercana()
    {
        // Busca robots en el contenedor que sean del jugador
        return _unitsContainer.GetChildren()
            .OfType<Robot>()
            .Where(r => r.EsDelJugador)
            .OrderBy(r => r.GlobalPosition.DistanceTo(this.GlobalPosition))
            .FirstOrDefault();
    }

    private TipoTropa ObtenerCounterIdeal(TipoTropa tipoAmenaza)
    {
        // Sistema de debilidades definido por ti
        return tipoAmenaza switch
        {
            TipoTropa.Artillero => TipoTropa.Ligero,     // Ligero gana a Artillero
            TipoTropa.Penetrador => TipoTropa.Artillero, // Artillero gana a Penetrador
            TipoTropa.Tanque => TipoTropa.Penetrador,    // Penetrador gana a Tanque
            TipoTropa.Ligero => TipoTropa.Tanque,        // Tanque gana a Ligero
            _ => TipoTropa.Ligero
        };
    }
}
