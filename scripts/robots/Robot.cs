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
        
        Detector.Enabled = true;
        Detector.CollideWithBodies = true;
        // --- CAMBIO CLAVE: AHORA DETECTA ÁREAS ---
        Detector.CollideWithAreas = true; 
        // -----------------------------------------
        Detector.AddException(this); 

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

        Detector.ForceRaycastUpdate(); 

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider() as Node;
            Robot otroRobot = objeto as Robot;

            // 1. Atacar Robot enemigo
            if (otroRobot != null && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                EstaAtacando = true;
                velocity.X = 0;
                EjecutarAtaque(() => otroRobot.RecibirDanio(this.Danio), (float)delta);
            }
            // 2. Atacar Base Enemiga (Nombres actualizados)
            else if (EsDelJugador && objeto.Name == "BaseEnemigo")
            {
                EstaAtacando = true;
                velocity.X = 0;
                EjecutarAtaque(() => Recursos.Instance.DanarEnemigo(this.Danio), (float)delta);
            }
            // 3. Atacar Base Jugador (Nombres actualizados)
            else if (!EsDelJugador && objeto.Name == "BaseJugador")
            {
                EstaAtacando = true;
                velocity.X = 0;
                EjecutarAtaque(() => Recursos.Instance.DanarJugador(this.Danio), (float)delta);
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

    private void EjecutarAtaque(Action dañoAccion, float delta)
    {
        TemporizadorAtaque += delta;
        if (TemporizadorAtaque >= 1.0f) 
        {
            dañoAccion.Invoke();
            TemporizadorAtaque = 0.0f;
        }
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
