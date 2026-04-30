using Godot;
using System;
using GameConstants;

public partial class Robot : CharacterBody2D
{
    // ... (Mantén tus variables de ExportGroup igual) ...
    [ExportGroup("Configuración Visual")]
    [Export] public bool IsFacingLeftByDefault = false;
    [Export] public Node2D Visual; 
    [Export] protected AnimatedSprite2D Anim;

    [ExportGroup("Atributos")]
    [Export] public TipoTropa Tipo; // Asegúrate de asignar esto en el Inspector para cada robot
    [Export] public bool EsDelJugador = true;
    [Export] public string Nombre;
    [Export] public int VidaMax = 100;
    public int VidaActual;
    
    [Export] public float Velocidad = 50.0f;
    [Export] public int DanioBase = 10; // Renombrado para claridad
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

        float direccionMovimiento = EsDelJugador ? 1.0f : -1.0f;
        Detector.TargetPosition = new Vector2(RangoAtaque * direccionMovimiento, 0);
        
        if (Visual != null)
        {
            Vector2 nuevaEscala = Visual.Scale;
            float orientacionFinal = EsDelJugador ? 1.0f : -1.0f;
            if (IsFacingLeftByDefault) orientacionFinal *= -1.0f;
            nuevaEscala.X = Mathf.Abs(nuevaEscala.X) * orientacionFinal;
            Visual.Scale = nuevaEscala;
        }

        if (Anim != null) Anim.Play("default");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity; 
        Detector.ForceRaycastUpdate(); 

        if (Detector.IsColliding())
        {
            var objeto = Detector.GetCollider() as Node;
            Robot otroRobot = objeto as Robot;

            bool objetivoEsRobotEnemigo = otroRobot != null && otroRobot.EsDelJugador != this.EsDelJugador;
            bool objetivoEsBaseEnemiga = (EsDelJugador && objeto.Name.ToString().Contains("BaseEnemigo")) || 
                                         (!EsDelJugador && objeto.Name.ToString().Contains("BaseJugador"));

            if (objetivoEsRobotEnemigo || objetivoEsBaseEnemiga)
            {
                velocity.X = 0;
                Velocity = velocity; 

                // Lógica de ataque con cálculo de daño corregido
                if (objetivoEsRobotEnemigo)
                    EjecutarAtaque(() => otroRobot.RecibirDanio(CalcularDanioFinal(otroRobot)), (float)delta);
                else
                    EjecutarAtaque(() => {
                        int danioABase = CalcularDanioFinal(null); // Daño reducido a bases
                        if (EsDelJugador) Recursos.Instance.DanarEnemigo(danioABase);
                        else Recursos.Instance.DanarJugador(danioABase);
                    }, (float)delta);

                MoveAndSlide();
                return; 
            }
        }

        float direccion = EsDelJugador ? 1.0f : -1.0f;
        Moverse(ref velocity, direccion);
        Velocity = velocity;
        MoveAndSlide();
    }

    // --- NUEVO MÉTODO PARA CALCULAR DAÑO ---
private int CalcularDanioFinal(Robot objetivo)
    {
        // 1. Si el objetivo es una Torre/Base: Reciben un 30% menos (Multiplicador 0.7)
        if (objetivo == null) 
        {
            return Mathf.RoundToInt(DanioBase * 0.7f);
        }

        float multiplicador = 1.0f;

        // 2. Lógica de Contra-tipos
        // VENTAJAS (Atacante hace +50% de daño)
        // Ligero > Artillero > Penetrador > Tanque > Ligero
        if (this.Tipo == TipoTropa.Ligero && objetivo.Tipo == TipoTropa.Artillero) multiplicador = 1.5f;
        else if (this.Tipo == TipoTropa.Artillero && objetivo.Tipo == TipoTropa.Penetrador) multiplicador = 1.5f;
        else if (this.Tipo == TipoTropa.Penetrador && objetivo.Tipo == TipoTropa.Tanque) multiplicador = 1.5f;
        else if (this.Tipo == TipoTropa.Tanque && objetivo.Tipo == TipoTropa.Ligero) multiplicador = 1.5f;

        // RESISTENCIAS (Objetivo recibe -20% de daño si es el "counter" natural)
        // Si el Ligero es atacado por el Tanque, el Ligero resiste.
        else if (objetivo.Tipo == TipoTropa.Ligero && this.Tipo == TipoTropa.Tanque) multiplicador = 0.8f;
        else if (objetivo.Tipo == TipoTropa.Artillero && this.Tipo == TipoTropa.Ligero) multiplicador = 0.8f;
        else if (objetivo.Tipo == TipoTropa.Penetrador && this.Tipo == TipoTropa.Artillero) multiplicador = 0.8f;
        else if (objetivo.Tipo == TipoTropa.Tanque && this.Tipo == TipoTropa.Penetrador) multiplicador = 0.8f;

        return Mathf.RoundToInt(DanioBase * multiplicador);
    }
    protected virtual void EjecutarAtaque(Action dañoAccion, float delta)
    {
        EstaAtacando = true;
        if (Anim != null && Anim.Animation != "atacar") 
            Anim.Play("atacar");

        TemporizadorAtaque += delta;
        if (TemporizadorAtaque >= 1.0f) 
        {
            dañoAccion.Invoke();
            TemporizadorAtaque = 0.0f;
        }
    }

    protected void Moverse(ref Vector2 velocity, float direccion)
    {
        EstaAtacando = false;
        TemporizadorAtaque = 0.0f;
        velocity.X = Velocidad * direccion;

        if (Anim != null && Anim.Animation != "default") 
            Anim.Play("default");
    }

    public virtual void RecibirDanio(int cantidad)
    {
        VidaActual -= cantidad;
        if (BarraVida != null) BarraVida.Value = VidaActual;
        if (VidaActual <= 0) Morir();
    }

    protected void Morir() => QueueFree();

    private void ConfigurarBarraVida()
    {
        if (BarraVida == null) return;
        StyleBoxFlat styleFill = new StyleBoxFlat { BgColor = EsDelJugador ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f) };
        BarraVida.AddThemeStyleboxOverride("fill", styleFill);
        BarraVida.MaxValue = VidaMax;
        BarraVida.Value = VidaActual;
        BarraVida.ShowPercentage = false;
    }
}
