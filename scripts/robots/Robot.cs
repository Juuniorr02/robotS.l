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
    [Export] public float RangoAtaque = 50.0f; 
    
    protected bool EstaAtacando = false;
    protected float TemporizadorAtaque = 0.0f;
    protected RayCast2D Detector;
    protected ProgressBar BarraVida;

    public override void _Ready()
    {
        // Configuración de movimiento para que no le afecte la gravedad
        MotionMode = MotionModeEnum.Floating;

        Detector = GetNode<RayCast2D>("RayCast2D");
        BarraVida = GetNodeOrNull<ProgressBar>("ProgressBar");
        
        Detector.Enabled = true;
        Detector.CollideWithBodies = true;
        Detector.CollideWithAreas = true; // Crucial para detectar tus bases Area2D
        Detector.AddException(this); 

        VidaActual = VidaMax;
        ConfigurarBarraVida();

        // Orientar el rayo hacia la derecha (jugador) o izquierda (enemigo)
        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccion, 0);
        
        // Orientar el gráfico (Visual)
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

    // 1. Definir el estilo de la barra llena (Verde)
    StyleBoxFlat styleFill = new StyleBoxFlat();
    styleFill.BgColor = new Color(0.2f, 0.8f, 0.2f); // Verde
    styleFill.SetCornerRadiusAll(2); // Bordes redondeados opcionales

    // 2. Definir el estilo del fondo (Rojo oscuro o Negro)
    StyleBoxFlat styleBg = new StyleBoxFlat();
    styleBg.BgColor = new Color(0.1f, 0.1f, 0.1f); // Gris casi negro
    styleBg.SetCornerRadiusAll(2);

    // 3. Aplicar los estilos a la ProgressBar
    BarraVida.AddThemeStyleboxOverride("fill", styleFill);
    BarraVida.AddThemeStyleboxOverride("background", styleBg);

    // 4. Configurar valores
    BarraVida.MaxValue = VidaMax;
    BarraVida.Value = VidaActual;
    BarraVida.ShowPercentage = false;
    
    // Asegurar que sea visible
    BarraVida.CustomMinimumSize = new Vector2(30, 4);
}

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        
        // --- BLOQUEO EJE Y ---
        // Esto asegura que el robot nunca suba ni baje, solo se mueva en X
        velocity.Y = 0; 

        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Detector.ForceRaycastUpdate(); 

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider() as Node;
            Robot otroRobot = objeto as Robot;

            // 1. Detección de Robots Enemigos
            if (otroRobot != null && otroRobot.EsDelJugador != this.EsDelJugador)
            {
                velocity.X = 0; // Detenerse
                EjecutarAtaque(() => otroRobot.RecibirDanio(this.Danio), (float)delta);
            }
            // 2. Detección de Bases (Usando Contains para mayor seguridad)
            else if (EsDelJugador && objeto.Name.ToString().Contains("BaseEnemigo"))
            {
                velocity.X = 0; // Detenerse
                EjecutarAtaque(() => Recursos.Instance.DanarEnemigo(this.Danio), (float)delta);
            }
            else if (!EsDelJugador && objeto.Name.ToString().Contains("BaseJugador"))
            {
                velocity.X = 0; // Detenerse
                EjecutarAtaque(() => Recursos.Instance.DanarJugador(this.Danio), (float)delta);
            }
            else
            {
                // Si lo que detecta es un aliado, sigue caminando (o podrías detenerlo si quieres fila)
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
        if (TemporizadorAtaque >= 1.0f) // Un golpe por segundo
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
