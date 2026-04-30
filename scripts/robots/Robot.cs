using Godot;
using System;
using GameConstants;

public partial class Robot : CharacterBody2D
{
    [Export] public TipoTropa Tipo;
    [Export] public bool EsDelJugador = true;
    [Export] public Node2D Visual; 

    [Export] public string Nombre;
    [Export] public int VidaMax = 100;
    public int VidaActual;
    
    [Export] public float Velocidad = 50.0f;
    [Export] public int Danio = 10;
    [Export] public float RangoAtaque = 40.0f;
    
    protected bool EstaAtacando = false;
    protected float TemporizadorAtaque = 0.0f;
    protected RayCast2D Detector;
    protected ProgressBar BarraVida;

    public override void _Ready()
    {
        Detector = GetNode<RayCast2D>("RayCast2D");
        BarraVida = GetNodeOrNull<ProgressBar>("ProgressBar");
        
        // --- CORRECCIÓN 1: SEGURIDAD DEL RAYCAST ---
        Detector.Enabled = true;
        Detector.CollideWithBodies = true;
        Detector.CollideWithAreas = false;
        Detector.AddException(this); // Esto obliga al rayo a ignorar al propio robot
        // -------------------------------------------

        VidaActual = VidaMax;
        ConfigurarBarraVida();

        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        
        if (Visual != null)
        {
            Vector2 nuevaEscala = Visual.Scale;
            float orientacion = EsDelJugador ? -1.0f : 1.0f; 
            nuevaEscala.X = Mathf.Abs(nuevaEscala.X) * orientacion;
            Visual.Scale = nuevaEscala;
        }
    }

    private void ConfigurarBarraVida()
    {
        if (BarraVida == null) return;
        BarraVida.Scale = Vector2.One; 
        BarraVida.CustomMinimumSize = new Vector2(30, 4);
        BarraVida.Size = new Vector2(30, 4);
        BarraVida.Position = new Vector2(-15, -35); 
        BarraVida.MaxValue = VidaMax;
        BarraVida.Value = VidaActual;
        BarraVida.ShowPercentage = false;

        StyleBoxFlat fill = new StyleBoxFlat { BgColor = new Color(0.2f, 0.8f, 0.2f) };
        StyleBoxFlat bg = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0.6f) };
        BarraVida.AddThemeStyleboxOverride("fill", fill);
        BarraVida.AddThemeStyleboxOverride("background", bg);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        float direccion = EsDelJugador ? 1.0f : -1.0f;

        // --- CORRECCIÓN 2: FORZAR ACTUALIZACIÓN ---
        Detector.ForceRaycastUpdate(); 
        // ------------------------------------------

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider();
            
            // Usamos "as Robot" para ser más flexibles con la herencia
            Robot otroRobot = objeto as Robot;

            if (otroRobot != null && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                EstaAtacando = true;
                velocity.X = 0;

                TemporizadorAtaque += (float)delta;
                if (TemporizadorAtaque >= 1.0f) 
                {
                    otroRobot.RecibirDanio(this.Danio);
                    TemporizadorAtaque = 0.0f;
                    GD.Print($"{this.Nombre} golpea a {otroRobot.Nombre}");
                }
            }
            else
            {
                Moverse(ref velocity, direccion);
            }
        }
        else
        {
            Moverse(ref velocity, direccion);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void Moverse(ref Vector2 velocity, float direccion)
    {
        EstaAtacando = false;
        TemporizadorAtaque = 0.0f;
        velocity.X = Velocidad * direccion;
    }

    public virtual void RecibirDanio(int cantidad)
    {
        VidaActual -= cantidad;
        if (BarraVida != null) BarraVida.Value = VidaActual;
        if (VidaActual <= 0) Morir();
    }

    protected void Morir() => QueueFree();
}
