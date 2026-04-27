using Godot;
using System;
using GameConstants;

public partial class Robot : CharacterBody2D
{
    [Export] public TipoTropa Tipo;
    [Export] public bool EsDelJugador = true;
    [Export] public Node2D Visual; 

    [Export] public string Nombre;
    [Export] public int Vida = 100;
    [Export] public float Velocidad = 50.0f;
    [Export] public int Danio = 10;
    [Export] public float RangoAtaque = 30.0f;
    
    protected bool EstaAtacando = false;
    protected RayCast2D Detector;

    public override void _Ready()
    {
        Detector = GetNode<RayCast2D>("RayCast2D");
        
        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        
        if (Visual != null)
        {
            Vector2 nuevaEscala = Visual.Scale;
            
            // --- CAMBIO AQUÍ ---
            // Invertimos la lógica: Si tus sprites originales miran a la izquierda,
            // el jugador (que va a la derecha) necesita escala negativa (-1) 
            // y el enemigo (que va a la izquierda) necesita escala positiva (1).
            float orientacion = EsDelJugador ? -1.0f : 1.0f; 
            
            nuevaEscala.X = Mathf.Abs(nuevaEscala.X) * orientacion;
            Visual.Scale = nuevaEscala;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        float direccion = EsDelJugador ? 1.0f : -1.0f;

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider();
            if (objeto is Robot otroRobot && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                EstaAtacando = true;
                velocity.X = 0;
                // Aquí podrías poner: if (Visual is AnimatedSprite2D anim) anim.Play("ataque");
            }
            else
            {
                EstaAtacando = false;
                velocity.X = Velocidad * direccion;
            }
        }
        else
        {
            EstaAtacando = false;
            velocity.X = Velocidad * direccion;
            // Aquí podrías poner: if (Visual is AnimatedSprite2D anim) anim.Play("caminar");
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public virtual void RecibirDanio(int cantidad)
    {
        Vida -= cantidad;
        if (Vida <= 0) Morir();
    }

    protected void Morir() => QueueFree();
}
