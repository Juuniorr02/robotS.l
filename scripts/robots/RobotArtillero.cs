using Godot;
using System;

public partial class RobotArtillero : Robot 
{
    public override void _Ready()
    {
        // 1. Definimos el rango ANTES del base._Ready para que el RayCast se cree largo
        RangoAtaque = 250.0f; 
        base._Ready();
        
        // 2. Forzamos la actualización del objetivo del rayo por si acaso
        if (Detector != null)
        {
            float direccion = EsDelJugador ? 1.0f : -1.0f;
            Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        }
    }

    protected override void EjecutarAtaque(Action dañoAccion, float delta)
    {
        // 3. Bloqueamos el movimiento para que no se llame a la animación "caminar"
        Velocity = Vector2.Zero; 
        EstaAtacando = true;

        // 4. Cambiamos la animación
        if (Anim != null && Anim.Animation != "disparar") 
        {
            Anim.Play("disparar");
        }

        TemporizadorAtaque += delta;
        if (TemporizadorAtaque >= 1.0f)
        {
            dañoAccion.Invoke();
            TemporizadorAtaque = 0.0f;
        }
    }
}
