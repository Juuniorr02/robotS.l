using Godot;
using System;
using GameConstants;

public partial class Robot : CharacterBody2D
{
    // --- REFERENCIAS Y CONFIGURACIÓN ---
    [Export] public TipoTropa Tipo;
    [Export] public bool EsDelJugador = true;
    
    // Arrastra aquí tu AnimatedSprite2D desde el inspector
    [Export] public Node2D Visual; 

    // --- ESTADÍSTICAS ---
    [Export] public string Nombre;
    [Export] public int Vida = 100;
    [Export] public float Velocidad = 50.0f;
    [Export] public int Danio = 10;
    [Export] public float RangoAtaque = 30.0f;
    
    protected bool EstaAtacando = false;
    protected RayCast2D Detector;

    public override void _Ready()
    {
        // 1. Configurar el RayCast2D
        Detector = GetNode<RayCast2D>("RayCast2D");
        
        // El jugador mira a la derecha (1), el enemigo a la izquierda (-1)
        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        
        // 2. Volteo Visual Seguro
        if (!EsDelJugador && Visual != null)
        {
            // Volteamos el nodo visual completo multiplicando la escala X por -1
            Vector2 nuevaEscala = Visual.Scale;
            nuevaEscala.X *= -1;
            Visual.Scale = nuevaEscala;
        }
        else if (Visual == null)
        {
            GD.PrintErr($"¡OJO! No has asignado el nodo Visual en el robot: {Name}");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        float direccion = EsDelJugador ? 1.0f : -1.0f;

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider();
            
            // Verificamos si es un Robot y si es del equipo contrario
            if (objeto is Robot otroRobot && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                EstaAtacando = true;
                velocity.X = 0;
                // Aquí podrías llamar a una función EjecutarAtaque()
            }
            else
            {
                // Si choca con un aliado o algo que no es un robot enemigo, sigue caminando
                EstaAtacando = false;
                velocity.X = Velocidad * direccion;
            }
        }
        else
        {
            EstaAtacando = false;
            velocity.X = Velocidad * direccion;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public virtual void RecibirDanio(int cantidad)
    {
        Vida -= cantidad;
        if (Vida <= 0) Morir();
    }

    protected void Morir()
    {
        QueueFree();
    }
}
