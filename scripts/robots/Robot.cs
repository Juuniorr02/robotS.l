using Godot;
using System;
using GameConstants;

public partial class Robot : CharacterBody2D
{
    [ExportGroup("Configuración Visual")]
    [Export] public bool IsFacingLeftByDefault = false; // Configura esto en el Inspector
    [Export] public Node2D Visual; 

    [ExportGroup("Atributos")]
    [Export] public TipoTropa Tipo;
    [Export] public bool EsDelJugador = true;
    [Export] public string Nombre;
    [Export] public int VidaMax = 100;
    public int VidaActual;
    
    [Export] public float Velocidad = 50.0f;
    [Export] public int Danio = 10;
    [Export] public float RangoAtaque = 50.0f; 
    
    protected bool EstaAtacando = false;
    protected float TemporizadorAtaque = 0.0f;
    protected RayCast2D Detector;
    protected ProgressBar BarraVida;

    public override void _Ready()
    {
        MotionMode = MotionModeEnum.Floating;
        Detector = GetNode<RayCast2D>("RayCast2D");
        BarraVida = GetNodeOrNull<ProgressBar>("ProgressBar");
        
        Detector.Enabled = true;
        Detector.CollideWithBodies = true;
        Detector.CollideWithAreas = true; 
        Detector.AddException(this); 

        VidaActual = VidaMax;
        ConfigurarBarraVida();

        // 1. Orientar el RayCast según el equipo
        float direccionMovimiento = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccionMovimiento, 0);
        
        // 2. LÓGICA DE VOLTEO (Inspirada en tu script de Enemy)
        if (Visual != null)
        {
            Vector2 nuevaEscala = Visual.Scale;
            
            // Si el jugador se mueve a la derecha (1.0) y el arte mira a la izquierda, hay que voltear (-1.0)
            // Si el enemigo se mueve a la izquierda (-1.0) y el arte mira a la izquierda, se queda igual (1.0)
            float orientacionFinal = EsDelJugador ? 1.0f : -1.0f;

            if (IsFacingLeftByDefault)
            {
                orientacionFinal *= -1.0f;
            }

            nuevaEscala.X = Mathf.Abs(nuevaEscala.X) * orientacionFinal;
            Visual.Scale = nuevaEscala;
        }
    }

    private void ConfigurarBarraVida()
    {
        if (BarraVida == null) return;

        // Estilos para que no salga gris
        StyleBoxFlat styleFill = new StyleBoxFlat();
        styleFill.BgColor = EsDelJugador ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
        
        StyleBoxFlat styleBg = new StyleBoxFlat();
        styleBg.BgColor = new Color(0.1f, 0.1f, 0.1f);

        BarraVida.AddThemeStyleboxOverride("fill", styleFill);
        BarraVida.AddThemeStyleboxOverride("background", styleBg);

        BarraVida.MaxValue = VidaMax;
        BarraVida.Value = VidaActual;
        BarraVida.ShowPercentage = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        velocity.Y = 0; 

        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.ForceRaycastUpdate(); 

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider() as Node;
            Robot otroRobot = objeto as Robot;

            if (otroRobot != null && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                velocity.X = 0;
                EjecutarAtaque(() => otroRobot.RecibirDanio(this.Danio), (float)delta);
            }
            else if (EsDelJugador && objeto.Name.ToString().Contains("BaseEnemigo"))
            {
                velocity.X = 0;
                EjecutarAtaque(() => Recursos.Instance.DanarEnemigo(this.Danio), (float)delta);
            }
            else if (!EsDelJugador && objeto.Name.ToString().Contains("BaseJugador"))
            {
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
        EstaAtacando = true;
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
