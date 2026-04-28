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
    [Export] public float RangoAtaque = 30.0f;
    
    protected bool EstaAtacando = false;
    protected RayCast2D Detector;
    protected ProgressBar BarraVida;

    public override void _Ready()
    {
        Detector = GetNode<RayCast2D>("RayCast2D");
        BarraVida = GetNode<ProgressBar>("ProgressBar");
        
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

    // 1. FORZAR ESCALA Y QUITAR RESTRICCIONES
    BarraVida.Scale = Vector2.One; // Fuerza escala 1:1
    BarraVida.CustomMinimumSize = new Vector2(30, 4); // Define el tamaño real
    BarraVida.Size = new Vector2(30, 4);
    
    // Esto evita que el nodo crezca si el texto (que está oculto) es grande
    BarraVida.ClipContents = true; 

    // 2. POSICIONAMIENTO MANUAL
    // Lo movemos a mano para que quede sobre el robot
    BarraVida.Position = new Vector2(-15, -30); 

    // 3. VALORES
    BarraVida.MaxValue = VidaMax;
    BarraVida.Value = VidaActual;
    BarraVida.ShowPercentage = false;

    // 4. ESTILO (Aquí forzamos que no tenga márgenes internos que lo inflen)
    StyleBoxFlat estiloRelleno = new StyleBoxFlat();
    estiloRelleno.BgColor = new Color(0.2f, 0.8f, 0.2f);
    estiloRelleno.ContentMarginBottom = 0;
    estiloRelleno.ContentMarginTop = 0;

    StyleBoxFlat estiloFondo = new StyleBoxFlat();
    estiloFondo.BgColor = new Color(0, 0, 0, 0.5f);
    estiloFondo.ContentMarginBottom = 0;
    estiloFondo.ContentMarginTop = 0;

    BarraVida.AddThemeStyleboxOverride("fill", estiloRelleno);
    BarraVida.AddThemeStyleboxOverride("background", estiloFondo);
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
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public virtual void RecibirDanio(int cantidad)
    {
        VidaActual -= cantidad;
        if (BarraVida != null) BarraVida.Value = VidaActual;
        if (VidaActual <= 0) Morir();
    }

    protected void Morir() => QueueFree();
}
