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
    private float _energiaPorSegundoBase = 12f; 
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
        float multiplicador = 1.0f + (_nivelEconomia * 0.6f);
        _energiaIA += _energiaPorSegundoBase * multiplicador * (float)delta;

        PensarJugada();
    }

    private void PensarJugada()
    {
        var amenazas = DetectarAmenazasCercanas();
        Robot amenazaPrincipal = amenazas.FirstOrDefault();
        float distanciaAmenaza = amenazaPrincipal != null ? GlobalPosition.DistanceTo(amenazaPrincipal.GlobalPosition) : 9999f;

        // --- NUEVA LÓGICA: SPAM DE EMERGENCIA ---
        // Si hay 3 o más unidades enemigas cerca, spammea Ligeros sin parar
        if (amenazas.Count >= 3 && distanciaAmenaza < 500f)
        {
            if (_energiaIA >= _costes[TipoTropa.Ligero])
            {
                DesplegarUnidad(TipoTropa.Ligero);
                GD.Print("[IA] ¡SOPORTANDO PRESIÓN! Spam de unidades ligeras.");
            }
            return; // Se centra solo en el spam hasta limpiar la zona
        }

        // --- PRIORIDAD 1: DEFENSA TÁCTICA ---
        if (amenazaPrincipal != null && distanciaAmenaza < 400f)
        {
            TipoTropa respuesta = ObtenerCounterIdeal(amenazaPrincipal.Tipo);
            if (_energiaIA >= _costes[respuesta])
            {
                DesplegarUnidad(respuesta);
            }
            return; 
        }

        // --- PRIORIDAD 2: ECONOMÍA ---
        int costeMejora = 100 + (_nivelEconomia * 150);
        if (_nivelEconomia < MaxNivelEconomia && _energiaIA >= costeMejora && distanciaAmenaza > 600f)
        {
            _energiaIA -= costeMejora;
            _nivelEconomia++;
            return;
        }

        // --- PRIORIDAD 3: ATAQUE PROACTIVO ---
        if (_energiaIA >= 250)
        {
            TipoTropa tropaAtaque = (amenazaPrincipal != null) ? ObtenerCounterIdeal(amenazaPrincipal.Tipo) : SeleccionarTropaAleatoria();
            DesplegarUnidad(tropaAtaque);

            // Apoyo extra si hay recursos
            if (_energiaIA > 80) DesplegarUnidad(TipoTropa.Ligero);
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

    // Cambiamos el método para detectar a TODOS los enemigos cerca, no solo uno
    private List<Robot> DetectarAmenazasCercanas()
    {
        return _unitsContainer.GetChildren()
            .OfType<Robot>()
            .Where(r => r.EsDelJugador && GlobalPosition.DistanceTo(r.GlobalPosition) < 600f)
            .OrderBy(r => GlobalPosition.DistanceTo(r.GlobalPosition))
            .ToList();
    }

    private TipoTropa SeleccionarTropaAleatoria()
    {
        float r = GD.Randf();
        if (r < 0.4f) return TipoTropa.Artillero;
        if (r < 0.7f) return TipoTropa.Tanque;
        return TipoTropa.Penetrador;
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
