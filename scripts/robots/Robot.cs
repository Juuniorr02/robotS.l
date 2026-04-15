using Godot;
using System;

// Definimos la clase base
public partial class Robot : CharacterBody2D
{
    // Variables que todos los robots comparten pero con valores distintos
    [Export] public string Nombre;
    [Export] public int Vida = 100;
    [Export] public float Velocidad = 50.0f;
    [Export] public int Danio = 10;
    [Export] public float RangoAtaque = 30.0f;
    
    public int Equipo = 1; // 1: Jugador, -1: Enemigo
    protected bool EstaAtacando = false;

    // Nodo para detectar enemigos (puedes usar un RayCast2D o Area2D)
    protected RayCast2D Detector;

    public override void _Ready()
    {
        // Buscamos un RayCast2D que añadiremos a la escena del robot
        Detector = GetNode<RayCast2D>("RayCast2D");
        // Orientamos el detector según el equipo
        Detector.TargetPosition = new Vector2(RangoAtaque * Equipo, 0);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Lógica de detección
        if (Detector.IsColliding())
        {
            var objetivo = Detector.GetCollider();
            // Aquí verificaremos si es un enemigo antes de atacar
            EstaAtacando = true;
            velocity.X = 0; 
            // Iniciar animación de ataque aquí
        }
        else
        {
            EstaAtacando = false;
            velocity.X = Velocidad * Equipo;
            // Iniciar animación de caminar aquí
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
        // Aquí podrías poner efectos de explosión
        QueueFree(); 
    }
}
