using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GameConstants;

public partial class BaseEnemigo : Node2D
{
    private Spawner _spawner;
    private Node _unitsContainer;

    private float _energiaIA = 0;
    private float _energiaPorSegundoBase = 12f; // Un poco más rápida que antes
    private int _nivelEconomia = 0;
    private const int MaxNivelEconomia = 4;

    private readonly Dictionary<TipoTropa, int> _costes = new() {
        { TipoTropa.Ligero, 50 },
        { TipoTropa.Tanque, 150 },
        { TipoTropa.Penetrador, 100 },
        { TipoTropa.Artillero, 200 }
    };

    public override void _Ready()
    {
        _spawner = GetNode<Spawner>("SpawnerEnemigo"); 
        _unitsContainer = GetParent().GetNode("UnitsContainer");
    }

    public override void _Process(double delta)
    {
        float multiplicador = 1.0f + (_nivelEconomia * 0.6f); // Mejora económica más potente
        _energiaIA += _energiaPorSegundoBase * multiplicador * (float)delta;

        PensarJugada();
    }

    private void PensarJugada()
    {
        Robot amenaza = DetectarAmenazaMasCercana();
        float distanciaAmenaza = amenaza != null ? GlobalPosition.DistanceTo(amenaza.GlobalPosition) : 9999f;

        // --- PRIORIDAD 1: DEFENSA CRÍTICA ---
        // Si el enemigo está muy cerca (menos de 400px), gasta TODO en counters
        if (amenaza != null && distanciaAmenaza < 400f)
        {
            TipoTropa respuesta = ObtenerCounterIdeal(amenaza.Tipo);
            if (_energiaIA >= _costes[respuesta])
            {
                DesplegarUnidad(respuesta);
            }
            return; // Bloquea otras acciones para centrarse en defender
        }

        // --- PRIORIDAD 2: ECONOMÍA ---
        int costeMejora = 100 + (_nivelEconomia * 150);
        if (_nivelEconomia < MaxNivelEconomia && _energiaIA >= costeMejora && distanciaAmenaza > 600f)
        {
            _energiaIA -= costeMejora;
            _nivelEconomia++;
            return;
        }

        // --- PRIORIDAD 3: ATAQUE PROACTIVO (PRESIÓN) ---
        // Si la IA tiene energía acumulada, lanza ataques para no dejarte respirar
        if (_energiaIA >= 250)
        {
            // Elige entre presionar con su counter o lanzar un ataque pesado
            TipoTropa tropaAtaque = (amenaza != null) ? ObtenerCounterIdeal(amenaza.Tipo) : SeleccionarTropaAleatoria();
            
            DesplegarUnidad(tropaAtaque);

            // "Doble Spawn": Si le sobra mucha energía, saca un Ligero de apoyo inmediatamente
            if (_energiaIA > 100) DesplegarUnidad(TipoTropa.Ligero);
        }
    }

    private void DesplegarUnidad(TipoTropa tipo)
    {
        if (_energiaIA >= _costes[tipo])
        {
            _energiaIA -= _costes[tipo];
            _spawner.Spawn(tipo);
        }
    }

    private TipoTropa SeleccionarTropaAleatoria()
    {
        // 40% Artillero (Presión fuerte), 60% Tanque o Penetrador
        float r = GD.Randf();
        if (r < 0.4f) return TipoTropa.Artillero;
        if (r < 0.7f) return TipoTropa.Tanque;
        return TipoTropa.Penetrador;
    }

    private Robot DetectarAmenazaMasCercana()
    {
        return _unitsContainer.GetChildren()
            .OfType<Robot>()
            .Where(r => r.EsDelJugador)
            .OrderBy(r => r.GlobalPosition.DistanceTo(this.GlobalPosition))
            .FirstOrDefault();
    }

    private TipoTropa ObtenerCounterIdeal(TipoTropa tipoAmenaza)
    {
        return tipoAmenaza switch
        {
            TipoTropa.Artillero => TipoTropa.Ligero,
            TipoTropa.Penetrador => TipoTropa.Artillero,
            TipoTropa.Tanque => TipoTropa.Penetrador,
            TipoTropa.Ligero => TipoTropa.Tanque,
            _ => TipoTropa.Ligero
        };
    }
}
