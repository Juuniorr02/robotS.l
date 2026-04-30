using Godot;
using System;

public partial class RobotArtillero : Robot 
{
    public override void _Ready()
    {
        // Rango largo para el artillero
        RangoAtaque = 250.0f; 
        base._Ready();
    }

    protected override void EjecutarAtaque(Action dañoAccion, float delta)
    {
        EstaAtacando = true;

        // El artillero usa "disparar" en lugar de "atacar"
        if (Anim != null && Anim.Animation != "disparar") 
            Anim.Play("disparar");

        TemporizadorAtaque += delta;
        if (TemporizadorAtaque >= 1.0f)
        {
            dañoAccion.Invoke();
            TemporizadorAtaque = 0.0f;
        }
    }
}
