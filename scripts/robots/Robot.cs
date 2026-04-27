using Godot;
using System;
using GameConstants; // Importante para usar el enum TipoTropa

public partial class Robot : CharacterBody2D
{
    // --- NUEVOS CAMPOS PARA LA IA ---
    [Export] public TipoTropa Tipo; // Para que la IA sepa qué tipo es
    [Export] public bool EsDelJugador = true; // Reemplazamos 'Equipo' por esto para claridad

    // --- TUS VARIABLES ORIGINALES ---
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
        
        // Ajustamos dirección: Jugador va a la derecha (+), Enemigo a la izquierda (-)
        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        
        // Opcional: Voltear el sprite si es enemigo
        if (!EsDelJugador) {
            GetNode<Sprite2D>("Sprite2D").FlipH = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        float direccion = EsDelJugador ? 1.0f : -1.0f;

        if (Detector.IsColliding())
        {
            // Verificamos si lo que golpea el RayCast es un enemigo
            var objeto = Detector.GetCollider();
            if (objeto is Robot otroRobot && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                EstaAtacando = true;
                velocity.X = 0;
                // Aquí llamarías a tu función de ataque
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

    protected void Morir() => QueueFree();
}
